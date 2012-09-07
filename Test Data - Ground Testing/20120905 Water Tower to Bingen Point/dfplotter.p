      # Gnuplot script file for plotting data in file "DeckerRox.txt"
      # This file is called   dfplotter.p
      set   autoscale                        # scale axes automatically
      unset log                              # remove any log-scaling
      unset label                            # remove any previous labels
      set xtic auto                          # set xtics automatically
      set ytic auto                          # set ytics automatically
      set title "Aerial Combat Emulation Logs 02SEP12"
      set xlabel "Longitude (deg)"
      set ylabel "Latitude (deg)"
     #set key 0.01,100
     #set label "Yield Point" at 0.003,260
     #set arrow from 0.0028,250 to 0.003,280
     #set xr [0.0:0.022]
     #set yr [0:325]
      plot    "DF001" using ($3/10000000):($2/10000000) title 'Location' with linespoints 
 #          , / "force.dat" using 1:3 title 'Beam' with points