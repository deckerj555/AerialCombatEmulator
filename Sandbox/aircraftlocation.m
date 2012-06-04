% Aircraft Location Algorithm
% J. Decker
% 30 Dec 2010
% 
% Attempting sperical vector field solutino
% calls: lla2ecef function
 %        ecef2neu function


clear all




% jesse's house 45.712662, -121.459911, 45 (m)
lat1 = 45.712662*pi/180;
lon1 = -121.459911*pi/180;
h1   = 45;

% jeff's house 45.7305, -121.512364, 150 (m)
lat2  = 45.73052*pi/180;
lon2 =  -121.512364*pi/180;
h2    = 150;

%lat2 = 0*pi()/180;
%lon2 = 90*pi/180;
%h2     =  0;



ac1 = [lat1, lon1, h1];
ac2 = [lat2, lon2, h2];

%transform geodectic coordinates to Cartesian Earth centered, Earth fixed
[ac1_xyz(1), ac1_xyz(2), ac1_xyz(3)]  = lla2ecef(ac1(1), ac1(2), ac1(3));
[ac2_xyz(1), ac2_xyz(2), ac2_xyz(3)]  = lla2ecef(ac2(1), ac2(2), ac2(3));

% calc the distance from ac1 to ac2
d = sqrt(sum((ac1_xyz - ac2_xyz).^2));

% assume some aircraft Euler angles [rads]
%phi     = 0/57.3;    %roll
%theta = -1.5/57.3;    %pitch
%psi       = -90/57.3;  %heading from -pi < phi < pi

% this is not a correct way to handle the problem. 	 
% see http://en.wikipedia.org/wiki/Vector_fields_in_cylindrical_and_spherical_coordinates#Spherical_coordinate_system
% deltax = d*sin(theta)*cos(psi)
% deltay = d*sin(theta)*sin(psi)
% deltaz = d*cos(pi/2 - theta)

% get deltas in the local tangent plane, North East Up
% pointingvector = ecef2neu(ac1, ac2, ac1_xyz, ac2_xyz);
pointingvector = ecef2neu(lat1, lon1, lat2, lon2, ac1_xyz(1), ac1_xyz(2), ac1_xyz(3), ac2_xyz(1), ac2_xyz(2), ac2_xyz(3));


east = pointingvector(1)
north = pointingvector(2)
up = pointingvector(3)

distance = sqrt( sum( pointingvector.^2))



theta = asin(up/(sqrt(east^2 + north^2)))*57.3
deltapsi = asin(north/ sqrt(east^2 + north^2))*57.3

theta2 = atan(up/east)*57.3
deltapsi2 = atan(north/east)*57.3


