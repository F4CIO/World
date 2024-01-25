ECHO you initially need to install tool with this line:  dotnet tool install -g NSwag.ConsoleCore
ECHO /output paramter from 

nswag openapi2tsclient /input:http://localhost:5265/swagger/v1/swagger.json /output:d:\Projects\InfoCompass.World\SourceCode\InfoCompass.World.UiAngular\src\app\shared\api-client.generated.ts /template:"angular" /useHttpClient:true
