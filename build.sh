#!/usr/bin/env bash
set -euo pipefail
dotnet restore WeatherCo.Producer
dotnet restore WeatherCo.MiddleTier
dotnet restore WeatherCo.UI
dotnet restore WeatherCo.Tests
dotnet build WeatherCo.Producer -c Release
dotnet build WeatherCo.MiddleTier -c Release
dotnet build WeatherCo.UI -c Release
dotnet test WeatherCo.Tests -c Release
