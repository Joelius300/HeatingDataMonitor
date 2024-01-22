"""
This file simply takes (parts of) an exported csv of the database (created using the regular backups)
and turns it into one that is in the original heating unit csv format so it can be used as input to the fake reader.
"""
import sys
from tqdm import tqdm

with open(sys.argv[1], "rt") as csvfile:
    with open(sys.argv[2], "wt") as newfile:
        for line in tqdm(csvfile):
            if not line.startswith("2"):
                continue  # WARNING: THIS WILL BREAK IN THE NEXT MILLENNIA :O

            line = line.strip()
            line = line.replace("f", "0")
            line = line.replace("t", "1")
            split = line.split(",")
            # split[1] is receive_time -> discarded
            sps_time = split[0]  # format: yyyy-mm-dd HH:MM:ss
            # must turn into format: dd.mm.yy;HH:MM:ss
            sps_split = sps_time.split(" ")
            split[0] = ".".join(reversed(sps_split[0][2:].split("-")))
            split[1] = sps_split[1]
            split.insert(5, "")  # Brennkammer
            # insert 3 empty values for: Betriebsart Fern. HK1;Verschiebung Fern. HK1;Freigabekontakt HK1;
            for i in range(3):
                split.insert(17, "")
            # continue with Vorlauf HK2 Ist at index 20

            # insert 15 empty values for: Betriebsart Fern. HK2;Verschiebung Fern. HK2;Freigabekontakt HK2;Vorlauf HK3 Ist;Vorlauf HK3 Soll;Betriebsphase HK3;Betriebsart Fern. HK3;Verschiebung Fern. HK3;Freigabekontakt HK3;Vorlauf HK4 Ist;Vorlauf HK4 Soll;Betriebsphase HK4;Betriebsart Fern. HK4;Verschiebung Fern. HK4;Freigabekontakt HK4
            for i in range(15):
                split.insert(23, "")
            # continues with Boiler_1 at index 38
            split.insert(39, "")  # Boiler 2
            # the rest is correct but there's a trailing semicolon in the original csv format for some reason
            line = ";".join(split) + ";"
            newfile.write(line + "\n")

print("done :)")
