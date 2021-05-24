import os
import sys

def rsp_patch():
    file_data = ""
    file = "../artifacts/rsp/14590475716575637239.rsp"
    old_str = ',--icf-iterations=5'
    with open(file, "r") as f:
        for line in f:
            if old_str in line:
                line = line.replace(old_str,'')
            file_data += line
    with open(file,"w") as f:
        f.write(file_data)
    return "rsp modified"
