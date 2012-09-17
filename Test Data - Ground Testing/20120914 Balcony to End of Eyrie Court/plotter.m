clear all

fid = fopen("DF001 End of Eyrie.txt", 'r');
header = fscanf(fid, '%s', [1, 16]);
fclose(fid);


%header = dlmread("DF001 End of Eyrie.txt", "\t", [0, 0, 0, 15 ]); %zero-indexed so don't read the header


data = dlmread("DF001 End of Eyrie.txt", "\t", 1, 0); %zero-indexed so don't read the header



