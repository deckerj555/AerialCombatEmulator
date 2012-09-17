%chdir "C:\\Users\\jeff\\Documents\\DogFighterRepo\\trunk\\Test Data - Ground Testing\\20120914 Balcony to End of Eyrie Court";


clear all;
close all;

%in order to read the header fields into seperate cell entries, i think we'll have to loop thru the header, starting off at the returned position from the last loop.
% [data, position, error] = fscanf(...)
%fid = fopen("DF001 End of Eyrie.txt", 'r');
%header = fscanf(fid, '%s', [1, 16]);
%fclose(fid);

%i just hard coded it to keep moving
header = {'GPSTime_csec'; 'Lat_e7'; 'Lon_e7'; 'Alt_cm'; 'PDOP_e2'; 'tSA_rad'; 'Yaw_mrad'; 'yawWithMagCal_mrad'; 'tSE_rad'; 'Pitch_mrad'; 'Distance_m'; 'EnemyLat_e7'; 'EnemyLon_e7'; 'EnemyAlt_cm'; 'EnemyPDOP_e2'; 'DLC'};
%1  = GPSTime_csec
%2  = Lat_e7
%3  = Lon_e7
%4  = Alt_cm
%5  = PDOP_e2
%6  = tSA_rad
%7  = Yaw_mrad
%8  = yawWithMagCal_mrad
%9  = tSE_rad
%10 = Pitch_mrad
%11 = Distance_m
%12 = EnemyLat_e7
%13 = EnemyLon_e7
%14 = EnemyAlt_cm
%15 = EnemyPDOP_e2
%16 = DLC

%note, i had already put # marks infront of all the rows i didn't want  gnuplot didn't look at them.  apparenly dlmread also ignores those lines.
data = dlmread("DF001 End of Eyrie.txt", "\t", 1, 0); %zero-indexed so don't read the header

%find all the non-zero entries for GPS time and get ride of them
nonZeroGpsTimes = find(data(:,1));
data = data(nonZeroGpsTimes,:);

GPSTime_csec       = data(:,1);
Lat_e7             = data(:,2);
Lon_e7             = data(:,3);
Alt_cm             = data(:,4);
PDOP_e2            = data(:,5);
tSA_rad            = data(:,6);
Yaw_mrad           = data(:,7);
yawWithMagCal_mrad = data(:,8);
tSE_rad            = data(:,9);
Pitch_mrad         = data(:,10);
Distance_m         = data(:,11);
EnemyLat_e7        = data(:,12);
EnemyLon_e7        = data(:,13);
EnemyAlt_cm        = data(:,14);
EnemyPDOP_e2       = data(:,15);
DLC                = data(:,16);


%figure;
%plot3(Lon_e7/10000000, Lat_e7/10000000, Alt_cm/100, 'b+-', EnemyLon_e7/10000000, EnemyLat_e7/10000000, EnemyAlt_cm/100, 'go-');

%right, ok, we can't work in the spherical coordinate system--to compare the toShootAz with Yaw_mrad and check the validity of the calculations we'll have to work in the NED frame--for all the same reasons we discovered earlier, namely LLA is a spherical coordinate system, therefore you can't superimpose range and bearing vectors
%plot(Lon_e7(1)/10000000, Lat_e7(1)/10000000, 'b+', EnemyLon_e7(1)/10000000, EnemyLat_e7(1)/10000000, 'go');

%LLA (aka geodetic) -> ECEF -> NED
%see http://en.wikipedia.org/wiki/Geodetic_system (16Sept2012)
%for simplcity i'm assigning just a scalar; intention is to put this in a loop and ultimately make plots for each data point, then assemble them into an animation
i = 200;
lat = Lat_e7(i)/10000000*pi/180;
lon = Lon_e7(i)/10000000*pi/180;
alt = Alt_cm(i)/100;
enemyLat = EnemyLat_e7(i)/10000000*pi/180;
enemyLon = EnemyLon_e7(i)/10000000*pi/180;
enemyAlt = EnemyAlt_cm(i)/100;

%%ECEF parameters
%a = 6378137.0;
%e2 = 6.69437999014e-3;
%N = a/sqrt((1 - e2)*(sin(lat))^2);
%%ECEF coordinates
%X = (N + alt)*cos(lat)*cos(lon);
%Y = (N + alt)*cos(lat)*sin(lon);
%Z = (N*(1 - e2) + alt)*sin(lat);
% just realized i have already done all this work....  should add an octave tools folder to the repo and then add that path to octave.


%need to double check this function--i'm pretty confident this one is correct
[X, Y, Z] = lla2ecef(lat, lon, alt);
[enemyX, enemyY, enemyZ] = lla2ecef(enemyLat, enemyLon, enemyAlt);

%need to double check this function--i'm not sure if this is the same as what we ended up coding in checkKillShot(); 
%copy 'n paste from checkKillShot:
%double east_m = -exMath.Sin(longitudeMe_rad) * delta_ECEF_X_cm / 100 + exMath.Cos(longitudeMe_rad) * delta_ECEF_Y_cm / 100;
%double north_m = -exMath.Cos(longitudeMe_rad) * exMath.Cos(latitudeMe_rad) * delta_ECEF_X_cm / 100 - exMath.Sin(latitudeMe_rad) * exMath.Sin(longitudeMe_rad) * delta_ECEF_Y_cm / 100 + exMath.Cos(latitudeMe_rad) * delta_ECEF_Z_cm / 100;
%double up_m = exMath.Cos(latitudeMe_rad) * exMath.Cos(longitudeMe_rad) * delta_ECEF_X_cm / 100 + exMath.Cos(latitudeMe_rad) * exMath.Sin(longitudeMe_rad) * delta_ECEF_Y_cm / 100 + exMath.Sin(latitudeMe_rad) * delta_ECEF_Z_cm / 100;

[pointingVector] = ecef2neu(lat, lon, enemyLat, enemyLon, X, Y, Z, enemyX, enemyY, enemyZ)






