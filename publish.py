#!/usr/bin/env python
# Script to publish for release on platforms in rid_list below.
import subprocess
import os
import zipfile
import shutil

# Clear previous dist files
try:
  shutil.rmtree(os.path.abspath("dist"))
except:
  pass

try:
  os.mkdir("dist", 0o666)
except:
  pass
  
assembly_name = "leaguenotifier"
static_files = [os.path.abspath("README"), os.path.abspath("LICENSE")]
project_list = [
  { 'folder': 'LeagueNotifier.Desktop/LeagueNotifier.Desktop.csproj', 'type': 'desktop' }, 
]
rid_list = ['win-x64']
zipfile_list = []

# Publish project e.g. desktop, console
for project in project_list:
  # Publish release for platform
  for rid in rid_list:
    result = subprocess.run(f"dotnet publish {project['folder']} -c Release -p:PublishSingleFile=true -o dist -r {rid} --no-self-contained", shell=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    
    out = result.stdout.decode('utf-8').rstrip()
    if out:
      print(out)
    err = result.stderr.decode('utf-8').rstrip()
    if err:
      print(err)
    
    files = [f for f in os.listdir(os.path.abspath("dist"))]
    
    zip_file = os.path.abspath(os.path.join("dist", f"{assembly_name}-{project['type']}-{rid}.zip"))
    if os.path.isfile(zip_file):
      os.remove(zip_file)
    
    with zipfile.ZipFile(zip_file, "w", zipfile.ZIP_DEFLATED) as archive:
      for s in static_files:
        archive.write(s, os.path.basename(s))
      for file in files:
        file_path = os.path.abspath(os.path.join("dist", file))
        if ".pdb" in file:
          os.remove(file_path)
          continue
        if file not in rid_list and file_path not in zipfile_list:
          archive.write(file_path, os.path.basename(file_path))
          os.remove(file_path)
    zipfile_list.append(zip_file)
    