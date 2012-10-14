%USAGE: [pointingvector] = ecef2neu(lat1, lon1, lat2, lon2, ac1_x, ac1_y, ac1_z, ac2_x, ac2_y, ac2_z);
% http://en.wikipedia.org/wiki/East_North_Up#ECEF_to.2Ffrom_ENU_Coordinates


function [pointingvector] = ecef2neu(lat1, lon1, lat2, lon2, ac1_x, ac1_y, ac1_z, ac2_x, ac2_y, ac2_z);

delta = [ac2_x - ac1_x;
         ac2_y - ac1_y;
         ac2_z - ac1_z];

% http://en.wikipedia.org/wiki/East_North_Up#ECEF_to.2Ffrom_ENU_Coordinates
A = [-sin(lon1),  cos(lon1),  0;
        -sin(lat1)*cos(lon1), -sin(lat1)*sin(lon1), cos(lat1);
        cos(lat1)*cos(lon1),  cos(lat1)*sin(lon1), sin(lat1)];

pointingvector = A*delta;

%copy 'n paste from checkKillShot 16Sept2012
%double east_m = -exMath.Sin(longitudeMe_rad) * delta_ECEF_X_cm / 100 + exMath.Cos(longitudeMe_rad) * delta_ECEF_Y_cm / 100;
%double north_m = -exMath.Cos(longitudeMe_rad) * exMath.Cos(latitudeMe_rad) * delta_ECEF_X_cm / 100 - exMath.Sin(latitudeMe_rad) * exMath.Sin(longitudeMe_rad) * delta_ECEF_Y_cm / 100 + exMath.Cos(latitudeMe_rad) * delta_ECEF_Z_cm / 100;
%double up_m = exMath.Cos(latitudeMe_rad) * exMath.Cos(longitudeMe_rad) * delta_ECEF_X_cm / 100 + exMath.Cos(latitudeMe_rad) * exMath.Sin(longitudeMe_rad) * delta_ECEF_Y_cm / 100 + exMath.Sin(latitudeMe_rad) * delta_ECEF_Z_cm / 100;

