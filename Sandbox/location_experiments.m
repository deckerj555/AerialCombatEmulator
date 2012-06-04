%let's experiment with differnt locations to see if we can get the proper ENU vectors.
%calls:  lla2ecef.m
%        ecef2neu.m
%j. decker
%2 feb 2012



lat_brdmn = 45.748349*pi/180;
lon_brdmn = -119.803693*pi/180;
h_brdmn = 192;

lat_arln = 45.720673*pi/180;
lon_arln = -120.179429*pi/180;
h_arln = 271;


lat_mtdefiance = 45.6486520*pi/180;
lon_mtdefiance = -121.7225020*pi/180;
h_mtdefiance = 150; %1512; 

% jeff's house 45.7305, -121.512364, 150 (m)
lat_jeff = 45.73052*pi/180;
lon_jeff =  -121.512364*pi/180;
h_jeff    = 150;

[x_brdmn, y_brdmn, z_brdmn] = lla2ecef(lat_brdmn, lon_brdmn, h_brdmn);
[x_arln, y_arln, z_arln]  = lla2ecef(lat_arln, lon_arln, h_arln);

[x_mtdef, y_mtdef, z_mtdef] = lla2ecef(lat_mtdefiance, lon_mtdefiance, h_mtdefiance)
[x_jeff, y_jeff, z_jeff] = lla2ecef(lat_jeff, lon_jeff, h_jeff)

delta_ecef_x = x_mtdef - x_jeff
delta_ecef_y = y_mtdef - y_jeff
delta_ecef_z = z_mtdef - z_jeff



pointingvector = ecef2neu(lat_jeff, lon_jeff, lat_mtdefiance, lon_mtdefiance, x_jeff, y_jeff, z_jeff, x_mtdef, y_mtdef, z_mtdef)

%pointingvector = ecef2neu(lat_brdmn, lon_brdmn, lat_arln, lon_arln, x_brdmn, y_brdmn, z_brdmn, x_arln, y_arln, z_arln)/1000;

atan2(pointingvector(1), pointingvector(2))

