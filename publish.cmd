nuget restore
msbuild Microsoft.Bot.Sample.SimpleEchoBot.sln -p:DeployOnBuild=true -p:PublishProfile=prod-finanzbot-Web-Deploy.pubxml -p:Password=C5cedK7AlBGDgircyjiWvcbrJPHdigvv985PJcQC6Niny9q5cRPvwsKnGRpK

