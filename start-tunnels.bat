@echo off
setlocal ENABLEDELAYEDEXPANSION

REM === Configuration ===
set FRONTEND_PORT=3000
set FRONTEND_SUBDOMAIN=myfrontend
set FRONTEND_AUTH=user:pass

set BACKEND_PORT=5247
set BACKEND_SUBDOMAIN=mybackend
set BACKEND_AUTH=admin:abcd1234

REM === Detect localtunnel CLI ===
where lt >nul 2>&1
if %errorlevel%==0 (
  echo Using global 'lt' command.
  set USE_NPX=0
) else (
  where npx >nul 2>&1
  if %errorlevel%==0 (
    echo 'lt' not found. Falling back to 'npx localtunnel'.
    set USE_NPX=1
  ) else (
    echo ERROR: Neither 'lt' nor 'npx' was found in PATH.
    echo        Install localtunnel globally:  npm i -g localtunnel
    echo        or use npx:                   npx localtunnel --help
    exit /b 1
  )
)

REM === Start tunnels in separate windows ===
if %USE_NPX%==0 (
  start "Frontend Tunnel" cmd /k lt --port %FRONTEND_PORT% --subdomain %FRONTEND_SUBDOMAIN% --auth %FRONTEND_AUTH%
  start "Backend Tunnel"  cmd /k lt --port %BACKEND_PORT%  --subdomain %BACKEND_SUBDOMAIN%  --auth %BACKEND_AUTH%
) else (
  start "Frontend Tunnel" cmd /k npx localtunnel --port %FRONTEND_PORT% --subdomain %FRONTEND_SUBDOMAIN% --auth %FRONTEND_AUTH%
  start "Backend Tunnel"  cmd /k npx localtunnel --port %BACKEND_PORT%  --subdomain %BACKEND_SUBDOMAIN%  --auth %BACKEND_AUTH%
)

echo.
echo Frontend tunnel: https://%FRONTEND_SUBDOMAIN%.loca.lt  (port %FRONTEND_PORT%)
echo Backend  tunnel: https://%BACKEND_SUBDOMAIN%.loca.lt   (port %BACKEND_PORT%)
echo.
echo Note: Keep the opened terminal windows running to keep tunnels alive.

endlocal
exit /b 0
