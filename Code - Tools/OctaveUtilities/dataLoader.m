function dataLoader
% FILE: dataLoader

% PURPOSE: Built to be a generic data loading script, the data can then be used with plotter.m



% TO DO (Copied from Plotter.m)
% [x] see the nasty-gram below about making a new directory in the repo and adding that to Octave's path
% [ ] add a +/- line to the tSA based on the deadband tolerance; shoud be done like it is in the code--magnitudes of deltas, and not just +/- yaw & +/- pitch, becuase the height difference  between the DFs would confuse the kill calc'd online and offline.
% [ ] make a sub-plot with the NEU data in the top and a yaw/yawWithMagCal/tsA time history in the bottom.  include a vertical line in the bottom plot that moves to indicate time progression
% [ ] so far i've started with all the data for NEU and indicated when we have a DLC = 1.  Start from the opposite direction: start with DLC = 1 data and only plot the NEU data for those moments.
% [ ] right now the header is hard-coded.  add step that reads in the header and sets the column data equal to the headers that are in the data file.
% =================================================================================================================================================================================================


hostname = gethostname();

if gethostname() == 'ace'
	%cd 'C:\\Users\\jeff\\Documents\\DogFighterRepo\\trunk\\Test Data - Ground Testing\\20120930 Water Tower to NorthBay';
	cd 'C:\\Users\\jeff\\Documents\\DogFighterRepo\\trunk\\Test Data - Ground Testing\\20131006 Short MagCal Check';
	presentWorkingDirectory = pwd;
	clc;
	printf('Good evening, Master Decker!\nYour working directory has been changed to: \n%s\n\n', presentWorkingDirectory);
	 
else
	cd = 'C:\Users\lowelln\Documents\DogFighterSVN\trunk\Test Data - Ground Testing\20120930 Water Tower to NorthBay';
	presentWorkingDirectory = pwd;
	clc;
	printf('Oh, for fuck''s sake, Lowell...\nyour shit''s probably in here: %s', presentWorkingDirectory);
	
	 
end



printf('Here are the txt files in your present working directory:\n')
dir *.txt
printf('\n')

fileName = input("\nPlease type filename: ", "s")


% Creates a cell array with containing header strings
fid = fopen(fileName,'r');
frewind(fid); 
firstLine = fgetl(fid);
tabs = find(firstLine == "\t");

header{1} = firstLine(1:tabs(1)-1);
for i = 2:length(tabs);
header{i} = firstLine(tabs(i-1)+1:tabs(i)-1);
end
header{i+1} = firstLine(tabs(i)+1:length(firstLine));
headerS = cellstr(header);

data = dlmread(fileName, "\t", 1, 0); %zero-indexed so don't read the header




% Get rid of zero-time entries for GPSTime
% NOTE: Assumes GPSTime is the first column.  Should go after the variables are assigned to their header names... but for now, this is one less forloop.
nonZeroGpsTimes = find(data(:,1));
data = data(nonZeroGpsTimes,:);

%Get rid of all the remaining data points that are reporting the Puli Township in Nantou County, Taiwan. 
nonCrapGpsSoluntions = find(data(:,2) ~= 240000000);
data = data(nonCrapGpsSoluntions, :);


% Assign columns to their header names as variables
for i = 1:length(header)
	assignin("base", headerS(i), data(:,i));	
end

endfunction













































































































