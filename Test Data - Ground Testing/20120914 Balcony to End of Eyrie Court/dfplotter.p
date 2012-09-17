      # Gnuplot script file for plotting data in file "20120905 DF001.txt"
      # This file is called   dfplotter.p
      set   autoscale                        # scale axes automatically
      unset log                              # remove any log-scaling
      unset label                            # remove any previous labels
      unset view
      set xtic auto                          # set xtics automatically
      set ytic auto                          # set ytics automatically
      set title "DF Logs 14SEP12 - Balcony"
      set xlabel "Longitude (deg)"
      set ylabel "Latitude (deg)"
     #set key 0.01,100
     #set label "Yield Point" at 0.003,260
     #set arrow from 0.0028,250 to 0.003,280
     #set xr [0.0:0.022]
     #set yr [0:325]
set term x11 0
set term png
set output 'fig.png'
set view 44, 338
#plot    "DF001 End of Eyrie.txt" using ($3/10000000):($2/10000000) title 'Car' with points, \
# "DF001 End of Eyrie.txt" u ($13/10000000):($12/10000000) t 'Boy Room' w points pt 6

splot    "DF001 End of Eyrie.txt" using ($3/10000000):($2/10000000):($4/100) title 'Car' with linespoints, \
 "DF001 End of Eyrie.txt" u ($13/10000000):($12/10000000):($14/100) t 'Balcony' w points pt 6

# set term x11 1
#  set xlabel "GPS Time (sec)"
#      set ylabel "Position Dilution of Precsion (-)"
# plot    "20120905 DF001.txt" using ($2/100):($4) title 'PDOP' with points pt 6;
 
