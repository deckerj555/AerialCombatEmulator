% working thru the Assuie's DTSO-TN-0640
%omega = 138.5deg;
%work through Eqn (3.7)

Rn0 = [-0.74896   -0.66262 0;
            0.66262    -0.74896   0;
            0                 0                   1];

%inital frames
N0 = [0 0 1]';
E0 = [0 1 0]';
D0 = [-1 0 0]';

%intermedia frames       
N1 = Rn0*N0;
E1 = Rn0*E0;
D1 = Rn0*D0;

%stymied trying to get R_{-E1}(alpha) matrix.

adel_lat = -34.9*pi/180;
adel_lon = 138.5*pi/180;
adel_h = 30000;

sydn_lat = -33.9*pi/180;
sydn_lon = 151.2*pi/180;
sydn_h = 30000;

[adel_x, adel_y, adel_z] = lla2ecef(adel_lat, adel_lon, adel_h);
[sydn_x, sydn_y, sydn_z] = lla2ecef(sydn_lat, sydn_lon, sydn_h);


%adel_xyz = [adel_x, adel_y, adel_z];
%sydn_xyz = [sydn_x, sydn_y, sydn_z];


%adel = [adel_lat, adel_lon];
%sydn = [sydn_lat, sydn_lon];

%pointingvector = ecef2neu(adel, sydn, adel_xyz, sydn_xyz)/1000
pointingvector = ecef2neu	(adel_lat, adel_lon, sydn_lat, sydn_lat, adel_x, adel_y, adel_z, sydn_x, sydn_y, sydn_z)

total_length = sqrt(pointingvector(1)^2 + pointingvector(2)^2 + pointingvector(3)^2)

%ok, so i'm not quite sure what's going on here, but the total_length disance is the same distance as what's calculated in the pdf, leading me to believe the we are actually using two differnt coordinate systems, both of which are right, just different.  
%i verified lla2ecef works correctly (via websites and the assuie's pdf), and i verifed the ecef2neu with google maps, given aircraftlocation.m (my house and jesse's house).  it should actually be written ENU...but whatevs.
%the aussies method of getting from ECEF to NED is very differnt. i can't follow what's happending. i lose it when you need to calculate R_{-E1}(\alpha). R_{N_0}(\omega} i get no problem--it's mainly subsitution--but the second iteration, that is, the second vector rotation is where it breaks down for me.
%the up shot is, I think the wikipedia ENU method will work.  i should verify its performance on the a few more points.
%still need to spit out the simple(r) trig that shows the if AC1 is facing AC2.  but that's easier if i know the ENU is indeed working relative to true north.