# Taken from: https://github.com/DiscordHooks/appveyor-discord-webhook
# License: MIT

$STATUS=$args[0]
$WEBHOOK_URL=$args[1]

if (!$WEBHOOK_URL) {
  Write-Output "WARNING!!"
  Write-Output "You need to pass the WEBHOOK_URL environment variable as the second argument to this script."
  Exit
}

$AVATAR="https://upload.wikimedia.org/wikipedia/commons/thumb/b/bc/Appveyor_logo.svg/256px-Appveyor_logo.svg.png"
$GITHUB_LOGO="https://assets-cdn.github.com/images/modules/logos_page/GitHub-Mark.png"
$LMP_LOGO="https://raw.githubusercontent.com/LunaMultiplayer/LunaMultiplayer/master/External/Logos/RedFont/LMP_Icon_Medium.png"

$REPO_URL="https://github.com/$env:APPVEYOR_REPO_NAME/"
$URL="https://ci.appveyor.com/project/$env:APPVEYOR_ACCOUNT_NAME/$env:APPVEYOR_PROJECT_NAME"

$ARTIFACTS_URL="https://ci.appveyor.com/project/$env:APPVEYOR_ACCOUNT_NAME/$env:APPVEYOR_PROJECT_NAME/build/job/$env:APPVEYOR_JOB_ID/artifacts"
$CLIENT_URL="https://ci.appveyor.com/api/buildjobs/$env:APPVEYOR_JOB_ID/artifacts/LunaMultiplayer-Client-Debug.zip"
$SERVER_WINDOWS_URL="https://ci.appveyor.com/api/buildjobs/$env:APPVEYOR_JOB_ID/artifacts/LunaMultiplayer-Server-win-x64-Debug.zip"
$SERVER_LINUX_URL="https://ci.appveyor.com/api/buildjobs/$env:APPVEYOR_JOB_ID/artifacts/LunaMultiplayer-Server-linux-x64-Debug.zip"
$MASTER_SERVER_URL="https://ci.appveyor.com/api/buildjobs/$env:APPVEYOR_JOB_ID/artifacts/LunaMultiplayerMasterServer-Debug.zip"
$BUILD_VERSION = [uri]::EscapeDataString($env:APPVEYOR_BUILD_VERSION)
$TIMESTAMP="$(Get-Date -format s)Z"

Switch ($STATUS) {
  "success" {
    $EMBED_COLOR=3066993
    $STATUS_MESSAGE="Passed"
    Break
  }
  "failure" {
    $EMBED_COLOR=15158332
    $STATUS_MESSAGE="Failed"
    Break
  }
  default {
    Write-Output "Default!"
    Break
  }
}

$WEBHOOK_DATA="{
  ""avatar_url"": ""$AVATAR"",
  ""embeds"": [ {
    ""color"": $EMBED_COLOR,
    ""author"": {
      ""name"": ""AppVeyor nightly build"",
      ""url"": ""$URL"",
      ""icon_url"": ""$AVATAR""
    },
    ""title"": ""New nightly build available for download - $env:APPVEYOR_REPO_BRANCH - $BUILD_VERSION"",
    ""url"": ""$ARTIFACTS_URL"",
    ""description"": ""Keep in mind that nightly builds might contain bugs! For all available builds click the link."",
    ""fields"": [
      {
        ""name"": ""Client"",
        ""value"": ""[Download Link]($CLIENT_URL)""
      },
      {
        ""name"": ""Server (Windows x64)"",
        ""value"": ""[Download Link]($SERVER_WINDOWS_URL)""
      },
      {
        ""name"": ""Server (Linux x64)"",
        ""value"": ""[Download Link]($SERVER_LINUX_URL)""
      }
    ],
    ""timestamp"": ""$TIMESTAMP"",
    ""footer"": {
      ""icon_url"": ""$LMP_LOGO"",
      ""text"": ""Luna Multiplayer""
    }
  } ]
}"

Write-Output "[Webhook]: Webhook payload data: $WEBHOOK_DATA"

Invoke-RestMethod -Uri "$WEBHOOK_URL" -Method POST -UserAgent "AppVeyor-Webhook" -ContentType "application/json" -Header @{"X-Autho" = "Dagger"} -Body $WEBHOOK_DATA

Write-Output "[Webhook]: Successfully sent the webhook."
