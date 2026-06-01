import re

broken_commands = [
    "!af", "!hpt", "!hpa", "!hpall", "!hpct", "!ka", "!iseli", "!td", "!tdb", "!fz", 
    "!ffmenu", "!komoyla", "!saklambac", "!box", "!ffdondur", "!gelt", "!gelct", 
    "!gelall", "!git", "!haksal", "!msay", "!fsay", "!kaccm", "!reloadconfig", 
    "!delay", "!sonsec", "!sonakalan", "!isyancilar", "!otores", "!otores0", 
    "!mct", "!mt", "!umt", "!umct", "!ut", "!uct", "!marker", "!w", "!uw", 
    "!rev", "!daire", "!diz", "!q", "!qq", "!topka", "!topkomutcu"
]

with open("JailBreakPlugin.cs", "r") as f:
    content = f.read()

registered_commands = re.findall(r'AddCommand\("css_([^"]+)"', content)
print("Registered:", registered_commands)

missing = []
for cmd in broken_commands:
    clean_cmd = cmd[1:] # remove !
    # check if clean_cmd is in registered_commands
    # some might be alternatives like !b for !box
    if clean_cmd not in registered_commands:
        missing.append(clean_cmd)

print("Missing:", missing)
