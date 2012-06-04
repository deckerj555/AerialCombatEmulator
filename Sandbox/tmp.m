%lat = 45.7305282*pi/180;
%lon = -121.5123265*pi/180;
%alt = 166.77;

yaw = linspace(-180, 180, 20);

good_yaw = mod((yaw + 360), 360);

both = [yaw; good_yaw]'