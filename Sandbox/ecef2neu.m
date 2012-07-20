%i can't even begin to explain what's going on in here.  just go to
% http://en.wikipedia.org/wiki/East_North_Up#ECEF_to.2Ffrom_ENU_Coordinates

%function [pointingvector] = ecef2neu(ac1, ac2, ac1_xyz, ac2_xyz)
function [pointingvector] = ecef2neu(lat1, lon1, lat2, lon2, ac1_x, ac1_y, ac1_z, ac2_x, ac2_y, ac2_z);

%delta = ac2_xyz - ac1_xyz;

delta = [ac2_x - ac1_x;
                 ac2_y - ac1_y;
                 ac2_z - ac1_z];

%lat1 = ac1(1);
%lon1 = ac1(2);

%lat2 = ac2(1);
%lon2 = ac2(2);

% http://en.wikipedia.org/wiki/East_North_Up#ECEF_to.2Ffrom_ENU_Coordinates
A = [-sin(lon1),  cos(lon1),  0;
        -sin(lat1)*cos(lon1), -sin(lat1)*sin(lon1), cos(lat1);
        cos(lat1)*cos(lon1),  cos(lat1)*sin(lon1), sin(lat1)];

pointingvector = A*delta;
