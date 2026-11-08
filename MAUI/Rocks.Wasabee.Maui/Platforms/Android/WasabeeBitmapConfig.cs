#if ANDROID
using Maui.GoogleMaps;
using Maui.GoogleMaps.Android.Factories;
using Rocks.Wasabee.Maui.Resources.Pins;
using SkiaSharp;
using SkiaSharp.Views.Android;
using System.Collections.Concurrent;
using System.Text;
using AndroidBitmapDescriptor = Android.Gms.Maps.Model.BitmapDescriptor;
using AndroidBitmapDescriptorFactory = Android.Gms.Maps.Model.BitmapDescriptorFactory;

namespace Rocks.Wasabee.Maui
{
    public sealed class WasabeeBitmapConfig : IBitmapDescriptorFactory
    {
        // Clés de type string : "res:<id>" pour les marqueurs, "pin:<couleur>" pour les pins.
        // Cela évite toute collision entre ids de ressources et valeurs de couleur.
        private static readonly ConcurrentDictionary<string, AndroidBitmapDescriptor> BitmapCache = new();

        // Tailles en dp (équivalent de 70x120 et 110x130 px sur un écran de densité 2,625, ex. Pixel 8).
        // Mettre ScaleWithDensity à false pour retrouver le comportement d'origine (pixels bruts,
        // et dans ce cas remettre 70/120 et 110/130).
        private const bool ScaleWithDensity = true;
        private const int MarkerWidth = 27;
        private const int MarkerHeight = 46;
        private const int PinWidth = 42;
        private const int PinHeight = 50;
        private const string DefaultPinColor = "#DD3D45"; // rouge

        private const string StatePending = "Pending";
        private const string StateAcknowledged = "Acknowledged";
        private const string StateCompleted = "Completed";
        private const string StateAssigned = "Assigned";

        private static readonly Dictionary<string, int> PlayerMarkers = new()
        {
            ["wasabee_player_marker"] = Resource.Drawable.wasabee_player_marker,
            ["wasabee_player_marker_self"] = Resource.Drawable.wasabee_player_marker_self,
            ["wasabee_player_marker_gray"] = Resource.Drawable.wasabee_player_marker_gray,
        };

        // Type d'opération -> (Pending, Acknowledged, Completed, Assigned)
        private static readonly Dictionary<string, (int Pending, int Acknowledged, int Completed, int Assigned)> OperationMarkers = new()
        {
            ["DestroyPortal"] = (
                Resource.Drawable.wasabee_markers_destroy_pending,
                Resource.Drawable.wasabee_markers_destroy_acknowledged,
                Resource.Drawable.wasabee_markers_destroy_completed,
                Resource.Drawable.wasabee_markers_destroy_assigned),
            ["UseVirus"] = (
                Resource.Drawable.wasabee_markers_virus_pending,
                Resource.Drawable.wasabee_markers_virus_acknowledged,
                Resource.Drawable.wasabee_markers_virus_completed,
                Resource.Drawable.wasabee_markers_virus_assigned),
            ["CapturePortal"] = (
                Resource.Drawable.wasabee_markers_capture_pending,
                Resource.Drawable.wasabee_markers_capture_acknowledged,
                Resource.Drawable.wasabee_markers_capture_completed,
                Resource.Drawable.wasabee_markers_capture_assigned),
            ["FarmPortal"] = (
                Resource.Drawable.wasabee_markers_farm_pending,
                Resource.Drawable.wasabee_markers_farm_acknowledged,
                Resource.Drawable.wasabee_markers_farm_completed,
                Resource.Drawable.wasabee_markers_farm_assigned),
            ["LetDecay"] = (
                Resource.Drawable.wasabee_markers_decay_pending,
                Resource.Drawable.wasabee_markers_decay_acknowledged,
                Resource.Drawable.wasabee_markers_decay_completed,
                Resource.Drawable.wasabee_markers_decay_assigned),
            ["MeetAgent"] = (
                Resource.Drawable.wasabee_markers_meetagent_pending,
                Resource.Drawable.wasabee_markers_meetagent_acknowledged,
                Resource.Drawable.wasabee_markers_meetagent_completed,
                Resource.Drawable.wasabee_markers_meetagent_assigned),
            ["Other"] = (
                Resource.Drawable.wasabee_markers_other_pending,
                Resource.Drawable.wasabee_markers_other_acknowledged,
                Resource.Drawable.wasabee_markers_other_completed,
                Resource.Drawable.wasabee_markers_other_assigned),
            ["RechargePortal"] = (
                Resource.Drawable.wasabee_markers_recharge_pending,
                Resource.Drawable.wasabee_markers_recharge_acknowledged,
                Resource.Drawable.wasabee_markers_recharge_completed,
                Resource.Drawable.wasabee_markers_recharge_assigned),
            ["UpgradePortal"] = (
                Resource.Drawable.wasabee_markers_upgrade_pending,
                Resource.Drawable.wasabee_markers_upgrade_acknowledged,
                Resource.Drawable.wasabee_markers_upgrade_completed,
                Resource.Drawable.wasabee_markers_upgrade_assigned),
            ["CreateLink"] = (
                Resource.Drawable.wasabee_markers_link_pending,
                Resource.Drawable.wasabee_markers_link_acknowledged,
                Resource.Drawable.wasabee_markers_link_completed,
                Resource.Drawable.wasabee_markers_link_assigned),
            ["Exclude"] = (
                Resource.Drawable.wasabee_markers_exclude_pending,
                Resource.Drawable.wasabee_markers_exclude_acknowledged,
                Resource.Drawable.wasabee_markers_exclude_completed,
                Resource.Drawable.wasabee_markers_exclude_assigned),
            ["GetKey"] = (
                Resource.Drawable.wasabee_markers_key_pending,
                Resource.Drawable.wasabee_markers_key_acknowledged,
                Resource.Drawable.wasabee_markers_key_completed,
                Resource.Drawable.wasabee_markers_key_assigned),
            ["GoToPortal"] = (
                Resource.Drawable.wasabee_markers_goto_pending,
                Resource.Drawable.wasabee_markers_goto_acknowledged,
                Resource.Drawable.wasabee_markers_goto_completed,
                Resource.Drawable.wasabee_markers_goto_assigned),
        };

        public AndroidBitmapDescriptor ToNative(BitmapDescriptor descriptor, IMauiContext mauiContext)
        {
            ArgumentNullException.ThrowIfNull(descriptor);
            ArgumentNullException.ThrowIfNull(mauiContext);

            var id = descriptor.Id;

            // 1. Marqueurs de joueurs
            if (PlayerMarkers.TryGetValue(id, out var playerIconId))
                return CreateMarker(playerIconId, mauiContext);

            // 2. Marqueurs d'opération : "<Type>|<État>"
            if (id.Contains('|'))
                return CreateMarker(ResolveOperationIcon(id), mauiContext);

            // 3. Pin coloré : "pin_<couleur>" ou rouge par défaut
            var pinColor = id.StartsWith("pin_", StringComparison.Ordinal)
                ? id[4..]
                : DefaultPinColor;

            return CreatePin(pinColor, mauiContext);
        }

        private static int ResolveOperationIcon(string id)
        {
            var parts = id.Split('|');
            if (parts.Length != 2)
                throw new ArgumentException($"Identifiant de marqueur invalide : '{id}'.", nameof(id));

            var (type, state) = (parts[0], parts[1]);

            if (!OperationMarkers.TryGetValue(type, out var icons))
                throw new ArgumentOutOfRangeException(nameof(id), type, "Type de marqueur inconnu.");

            return state switch
            {
                StatePending => icons.Pending,
                StateAcknowledged => icons.Acknowledged,
                StateCompleted => icons.Completed,
                StateAssigned => icons.Assigned,
                _ => throw new ArgumentOutOfRangeException(nameof(id), state, "État de marqueur inconnu.")
            };
        }

        private static AndroidBitmapDescriptor CreateMarker(int iconId, IMauiContext mauiContext) =>
            BitmapCache.GetOrAdd($"res:{iconId}", _ =>
            {
                var context = mauiContext.Context
                    ?? throw new InvalidOperationException("Le contexte Android est indisponible.");
                var resources = context.Resources
                    ?? throw new InvalidOperationException("Les ressources Android sont indisponibles.");

                using var stream = resources.OpenRawResource(iconId);
                return CreateDescriptor(stream, MarkerWidth, MarkerHeight, GetDensity(mauiContext));
            });

        private static AndroidBitmapDescriptor CreatePin(string color, IMauiContext mauiContext) =>
            BitmapCache.GetOrAdd($"pin:{color}", _ =>
            {
                var pin = new WasabeePinSvg(color);
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(pin.RawData));
                return CreateDescriptor(stream, PinWidth, PinHeight, GetDensity(mauiContext));
            });

        private static AndroidBitmapDescriptor CreateDescriptor(Stream svgStream, int width, int height, float density)
        {
            using var svg = new Svg.Skia.SKSvg();
            var picture = svg.Load(svgStream)
                ?? throw new InvalidOperationException("Impossible de charger le SVG.");

            using var bitmap = RenderPicture(picture, (int)(width * density), (int)(height * density));
            return AndroidBitmapDescriptorFactory.FromBitmap(bitmap.ToBitmap());
        }

        // Rend le SVG directement à la taille cible (pas de rastérisation puis redimensionnement).
        private static SKBitmap RenderPicture(SKPicture picture, int width, int height)
        {
            var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Transparent);

            var bounds = picture.CullRect;
            canvas.Scale(width / bounds.Width, height / bounds.Height);
            canvas.Translate(-bounds.Left, -bounds.Top);
            canvas.DrawPicture(picture);

            return bitmap;
        }

        private static float GetDensity(IMauiContext mauiContext)
        {
            if (!ScaleWithDensity)
                return 1f;

            return mauiContext.Context?.Resources?.DisplayMetrics?.Density ?? 1f;
        }
    }
}
#endif