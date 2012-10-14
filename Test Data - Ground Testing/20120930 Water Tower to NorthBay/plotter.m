% FILE: plotter.m

% PURPOSE: Built to be a generic plotting script used to post-process DogFighter logs to evaluate the pointing performance to each unit.  (huh huh...<Tevin voice> "unit")

% CALLS: lla2ecef.m and ecef2neu.m are the only non-standard funcitons

% TO DO:
% [x] see the nasty-gram below about making a new directory in the repo and adding that to Octave's path
% [ ] add a +/- line to the tSA based on the deadband tolerance; shoud be done like it is in the code--magnitudes of deltas, and not just +/- yaw & +/- pitch, becuase the height difference  between the DFs would confuse the kill calc'd online and offline.
% [ ] make a sub-plot with the NEU data in the top and a yaw/yawWithMagCal/tsA time history in the bottom.  include a vertical line in the bottom plot that moves to indicate time progression
% [ ] so far i've started with all the data for NEU and indicated when we have a DLC = 1.  Start from the opposite direction: start with DLC = 1 data and only plot the NEU data for those moments.
% [ ] right now the header is hard-coded.  add step that reads in the header and sets the column data equal to the headers that are in the data file.
% =================================================================================================================================================================================================

% copy/paste into Octave to change the working directory.
%chdir "C:\\Users\\jeff\\Documents\\DogFighterRepo\\trunk\\Test Data - Ground Testing\\20120930 Water Tower to NorthBay";

%clear all;
close all;

%in order to read the header fields into seperate cell entries, i think we'll have to loop thru the header, starting off at the returned position from the last loop.
% [data, position, error] = fscanf(...)
%fid = fopen("DF001 End of Eyrie.txt", 'r');
%header = fscanf(fid, '%s', [1, 16]);
%fclose(fid);

%i just hard coded it to keep moving
header = {'GPSTime_csec'; 'Lat_e7'; 'Lon_e7'; 'Alt_cm'; 'PDOP_e2'; 'tSA_rad'; 'Yaw_mrad'; 'yawWithMagCal_mrad'; 'tSE_rad'; 'Pitch_mrad'; 'Distance_m'; 'EnemyLat_e7'; 'EnemyLon_e7'; 'EnemyAlt_cm'; 'EnemyPDOP_e2'; 'DLC'; 'MagCalCounter'};
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
%17 = MagCalCounter

%don't bother loading the file, if the workspace is already populated (at least with GPSTime_csec).
if ~exist('GPSTime_csec', 'var')
	data = dlmread("20120930 - DF001 at WaterTower.txt", "\t", 1, 0); %zero-indexed so don't read the header
end
	

%find all the zero-time entries for GPS time and get rid of them
nonZeroGpsTimes = find(data(:,1));
data = data(nonZeroGpsTimes,:);

%find all the remaining data points that are reporting the Puli Township in Nantou County, Taiwan. 
nonCrapGpsSoluntions = find(data(:,2) ~= 240000000);
data = data(nonCrapGpsSoluntions, :);

GPSTime_csec         = data(:,1);
Lat_e7               = data(:,2);
Lon_e7               = data(:,3);
Alt_cm               = data(:,4);
PDOP_e2              = data(:,5);
tSA_rad              = data(:,6);
Yaw_mrad             = data(:,7);
yawWithMagCal_mrad   = data(:,8);
tSE_rad              = data(:,9);
Pitch_mrad           = data(:,10);
Distance_m           = data(:,11);
EnemyLat_e7          = data(:,12);
EnemyLon_e7          = data(:,13);
EnemyAlt_cm          = data(:,14);
EnemyPDOP_e2         = data(:,15);
DLC                  = data(:,16);
MagCalCounter        = data(:,17);
time_sec             = (GPSTime_csec - GPSTime_csec(1))/100;
dayOfWeekZulu        = 1; %testing conducted sunday at 2200L, so monday 0500Z, or the second day of the gps week which is zero-indexed. sunday = 0, monday = 1       
gpsClockTimeZulu_hrs = (GPSTime_csec/100 - dayOfWeekZulu*24*3600)/3600;
deadband_deg         = 175/1000*180/pi; %this data set was taken with rev 139;

% // Metal Detector Magic!!!!! (like finding a gold coin on the beach)
% double maxOfAzimuths = exMath.Max(toShootAzimuth_rad * 1000, yawWithMagCal_mrad);
% double minOfAzimuths = exMath.Min(toShootAzimuth_rad * 1000, yawWithMagCal_mrad);
% double azimuthDelta_mrad = exMath.Min((2 * exMath.PI * 1000 - maxOfAzimuths) + minOfAzimuths, maxOfAzimuths - minOfAzimuths); // always pos by nature.
% double elevationDelta_mrad = toShootElevationAngle_rad * 1000 - positionMe.Pitch_mrad;
% double magnitudeOfDeltas_mrad = System.Math.Pow(azimuthDelta_mrad * azimuthDelta_mrad + elevationDelta_mrad * elevationDelta_mrad, 0.5);
%Vector math, FTW!
%hmmm...we're all fuck up here... we know yawWithMagCal_mrad doesn't wrap about 2pi properly. this flows down into the magnitudeOfDeltas.  how did we ever detect a kill?
%lots of plots for debug
maxOfAzimuths = max(tSA_rad * 1000, yawWithMagCal_mrad);
minOfAzimuths = min(tSA_rad * 1000, yawWithMagCal_mrad);
azimuthDelta_mrad = min((2*pi*1000 - maxOfAzimuths) + minOfAzimuths, maxOfAzimuths - minOfAzimuths);
elevationDelta_mrad = tSE_rad*1000 - Pitch_mrad;
magnitudeOfDeltas_mrad = sqrt(azimuthDelta_mrad.^2 + elevationDelta_mrad.^2);
DLC_check_indices = find(magnitudeOfDeltas_mrad < deadband_deg/180*pi*1000);
DLC_check = zeros(length(GPSTime_csec), 1);
DLC_check(DLC_check_indices) = 1;


figure;
plot(maxOfAzimuths/1000*180/pi);
title('maxOfAzimuths_deg');
grid;
%print -dpng maxOfAzimuths.png

figure;
plot(minOfAzimuths/1000*180/pi)
title('minOfAzimuths_deg')
grid;
%print -dpng minOfAzimuths.png

figure;
plot(azimuthDelta_mrad/1000*180/pi)
title('azimuthDelta_deg')
grid;
%print -dpng azimuthDelta.png

figure;
plot(elevationDelta_mrad/1000*180/pi)
title('elevationDelta_deg')
grid;
%print -dpng elevationDelta.png

figure;
plot(magnitudeOfDeltas_mrad/1000*180/pi)
title('magnitudeOfDeltas_deg')
grid;
%print -dpng magnitudeOfDeltas.png

figure
hold on
plot([1:length(maxOfAzimuths)], maxOfAzimuths/1000*180/pi, [1:length(minOfAzimuths)], minOfAzimuths/1000*180/pi, [1:length(azimuthDelta_mrad)], azimuthDelta_mrad/1000*180/pi, [1:length(magnitudeOfDeltas_mrad)], magnitudeOfDeltas_mrad/1000*180/pi)
plot(get(gca, 'xlim'), [deadband_deg, deadband_deg], 'r', get(gca, 'xlim'), [-deadband_deg, -deadband_deg], 'r')
hold off
grid
legend('maxOfAzimuths', 'minOfAzimuths', 'azimuthDelta', 'magnitudeOfDeltas')
ylabel('[deg]')
xlabel('tick')
title('Comparison of CheckKillShot Calculation Terms')
%print -dfig ComparisonOfKillCalcTerms.fig

%it's two lines lie right over top of each other
figure
hold on
plot([1:length(maxOfAzimuths)], maxOfAzimuths/1000*180/pi, [1:length(minOfAzimuths)], minOfAzimuths/1000*180/pi, [1:length(azimuthDelta_mrad)], azimuthDelta_mrad/1000*180/pi, [1:length(magnitudeOfDeltas_mrad)], magnitudeOfDeltas_mrad/1000*180/pi, [1:length(yawWithMagCal_mrad/1000*180/pi)], yawWithMagCal_mrad/1000*180/pi, '-.')
plot(get(gca, 'xlim'), [deadband_deg, deadband_deg], 'r', get(gca, 'xlim'), [-deadband_deg, -deadband_deg], 'r')
hold off
grid
legend('maxOfAzimuths', 'minOfAzimuths', 'azimuthDelta', 'magnitudeOfDeltas', 'yawWithMagCal')
ylabel('[deg]')
xlabel('tick')
title('Comparison of CheckKillShot Calculation Terms')
%print -dfig ComparisonOfKillCalcTermsWithYaw.fig

%21:58, 22:12 written down in Lowell's notes
magCalTimeFromNotesZ   = [21.97+7-24, 22.20+7-24];

%detect a step change in the difference between yaw and yawWithMagCal; *should* be only when there's a magCal.
temp = fix(diff(Yaw_mrad - yawWithMagCal_mrad)); %fix rounds towared zero to get rid of the e-13 results from round off error
magCal_indicies = find( temp);

%use this line to hide the plot display so it's not drawn every time, speeds up the for-loop; handy when you're saving off 300 individual picture files
%set(0, 'defaultfigurevisible', 'off')

for i = magCal_indicies(1) - 5 : magCal_indicies(1) + 10;  %start looking at NEU data just before magCal and just after
%for i = 20:20;  %by manual inspection the data doens't really start looking nice until around index 20
    
	disp(i);
	
	lat = Lat_e7(i)/10000000*pi/180;
	lon = Lon_e7(i)/10000000*pi/180;
	alt = Alt_cm(i)/100;
	enemyLat = EnemyLat_e7(i)/10000000*pi/180;
	enemyLon = EnemyLon_e7(i)/10000000*pi/180;
	enemyAlt = EnemyAlt_cm(i)/100;
	
	[X, Y, Z] = lla2ecef(lat, lon, alt);
	[enemyX, enemyY, enemyZ] = lla2ecef(enemyLat, enemyLon, enemyAlt);
	[pointingVector] = ecef2neu(lat, lon, enemyLat, enemyLon, X, Y, Z, enemyX, enemyY, enemyZ);
	north_m = pointingVector(2);
	east_m = pointingVector(1);
	up_m = pointingVector(3);
	
	distanceCheck = sqrt(pointingVector(1)^2 + pointingVector(2)^2 + pointingVector(3)^2);
	distanceErrorPercent = (distanceCheck - Distance_m(i))/Distance_m(i);
	
	toShootAzimuthCheck = atan2(east_m, north_m);
	tSAErrorCheckPercentage = (tSA_rad(i) - toShootAzimuthCheck)/tSA_rad(i);
	
	
	figure;
	hold on;
	plot(0,0, 'go', east_m, north_m, 'bo');%, 'displayname', 'Offline tSA');
	plot([0, Distance_m(i)*sin(tSA_rad(i))], [0,Distance_m(i)*cos(tSA_rad(i))], 'color', 'green', 'linestyle', '--', 'marker', 'x', 'DisplayName', 'toShootAzimuth');
	plot([0,Distance_m(i)*sin(yawWithMagCal_mrad(i)/1000)], [0, Distance_m(i)*cos(yawWithMagCal_mrad(i)/1000)], 'color', 'red', 'linestyle', '-', 'DisplayName', 'YawWithMagCal');
	%fill a box with red if a DLC = 1 was logged--this means the Me DogFighter be killin a muthafucka.
	if(DLC(i))
		fill([-10 10 10 -10 ], [-10 -10 10 10], 'r')
	end
	%show a when the magCal was done (in reality, the corrected data won't show until the next time step)
	if(find(i == magCal_indicies));
		text(-90, 70, 'MagCal''d now!!!')
	end
		
	hold off;
	grid
	xlabel('East [m]');
	ylabel('North [m]');
	title({'20120930 Water Tower to NorthBay';'NEU Frame Geometry Check'});
	%axis([-120 120 -50 50])
	legend('Me','Enemy','toShootAzimuth','yawWithMagCal')
	%text(-90, 30, {'online/offline tSA Error%:', num2str(tSAErrorCheckPercentage*100)}  );
	text(-90, 40, ['Zulu Time: ', num2str(gpsClockTimeZulu_hrs(i)), ' hrs']);
	text(-90, 20, ['Elapsed Test Time: ', num2str(time_sec(i)), ' sec']);
	%eval(['print -dpng [plots/jeff_' num2str(i) '.png']])
	%close(gcf);
	
end

%i don't think Octave has support for cells--so i've just been moving a return statement around.
return;

figure;
hold on;
plot(gpsClockTimeZulu_hrs, yawWithMagCal_mrad*.180/pi, 'b-+', gpsClockTimeZulu_hrs, Yaw_mrad*.180/pi, 'g--o', gpsClockTimeZulu_hrs, tSA_rad*180/pi, 'r-');
ylimits = get(gca, 'ylim');
%plot([magCalTimeFromNotesZ(1), magCalTimeFromNotesZ(1)], ylimits, 'k-', [magCalTimeFromNotesZ(2), magCalTimeFromNotesZ(2)], ylimits, 'k-');
plot([gpsClockTimeZulu_hrs(magCal_indicies(1)), gpsClockTimeZulu_hrs(magCal_indicies(1))], ylimits, 'c--', [gpsClockTimeZulu_hrs(magCal_indicies(2)), gpsClockTimeZulu_hrs(magCal_indicies(2))], ylimits, 'c--');
xlimits = get(gca, 'xlim');
plot(xlimits, [360, 360], 'g--');
hold off
grid
xlabel('Clock Time from GPS Zulu [hrs.hrs]');
ylabel('Yaw & YawWithMagCal [deg]');
title({'20120930 Water Tower to NorthBay';'Yaw, YawWithMagCal, and toShootAzimuth'});
legend('YawWithMagCal', 'Yaw', 'tSA', 'magCal')
text(4.65, 370, '360deg');
%print -dpng magCalBehavior.png
%close(gcf);

% figure;
% plot(diff(GPSTime_csec/100));
% ylabel('\Delta GPS Time [sec]', 'interpreter', 'tex');
% xlabel('Sample [-]');
% title({'20120930 Water Tower to NorthBay';'Logging Frequency Check'});
% grid;
%print -dpng LoggingFrequencyCheck.png
%close(gcf);


