FROM mcr.microsoft.com/dotnet/sdk:6.0 AS publish
WORKDIR /src
COPY ["Ingress/Ingress.csproj", "Ingress/"]
RUN dotnet restore "Ingress/Ingress.csproj"
COPY . .
WORKDIR "/src/Ingress"

RUN dotnet publish "Ingress.csproj" -c Release -o /app/publish --self-contained -r linux-x64

#FROM mcr.microsoft.com/dotnet/runtime:6.0
FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.6-cbl-mariner2.0-distroless-amd64
WORKDIR /app
COPY --from=publish /app/publish .
ENV PATH=/app:$PATH
USER nobody

ENTRYPOINT ["Ingress"]