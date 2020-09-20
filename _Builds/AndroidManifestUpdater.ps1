param(
        [parameter(Position=0,Mandatory=$true)]
        [string] $filePath,
        [parameter(Position=1,Mandatory=$true)]
        [string] $package,
        [parameter(Position=2,Mandatory=$true)]
        [string] $versionName,
        [parameter(Position=3,Mandatory=$true)]
        [string] $label,
        [parameter(Position=4,Mandatory=$false)]
        [string] $icon
    )

Write-Output "Previous file content :"
Write-Output [xml](Get-Content $filePath)
Write-Output "------------------------------------------"

$xml = [xml](Get-Content $filePath)
$timeStamp = ([DateTimeOffset](Get-Date)).ToUnixTimeSeconds()

$xpath = "//manifest"
Write-Output "Writing to $filePath - setting manifest.package to $package"
Write-Output "Writing to $filePath - setting manifest.android:versionName to $versionName"
Write-Output "Writing to $filePath - setting manifest.android:versionCode to $timeStamp"
Select-Xml -xml $xml -XPath $xpath |
%{              
    $_.Node.SetAttribute("package", $package)
    $_.Node.SetAttribute("android:versionName", $versionName)
    $_.Node.SetAttribute("android:versionCode", $timeStamp)
}

$xpath = "//manifest/application"
Write-Output "Writing to $filePath - setting manifest.application.android:label to $label"
Select-Xml -xml $xml -XPath $xpath |
%{
    $_.Node.SetAttribute("android:label", $label)
    if ($icon)
    {
        Write-Output "Writing to $filePath - setting manifest.application.android:icon to $icon"
        $_.Node.SetAttribute("android:icon", $icon)
    }
}

$xml.save($filePath)

Write-Output "------------------------------------------"
Write-Output "New file content :"
Write-Output [xml](Get-Content $filePath)