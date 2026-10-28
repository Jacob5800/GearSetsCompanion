from pathlib import Path
import json,zipfile,time
p=Path(__file__).resolve().parents[1]
d=p/'bin/Release'; manifest=json.loads((d/'GearSetsCompanion.json').read_text(encoding='utf-8-sig'));manifest['LastUpdate']=int(time.time())
notes=json.loads((p/'GearSetsCompanion.json').read_text(encoding='utf-8'))['Changelog'];manifest['Changelog']=notes
(d/'GearSetsCompanion.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
with zipfile.ZipFile(d/'GearSetsCompanion.zip','w',zipfile.ZIP_DEFLATED)as z:
 for name in ['GearSetsCompanion.dll','GearSetsCompanion.deps.json','GearSetsCompanion.json']:z.write(d/name,name)
entry=dict(manifest);url='https://github.com/Jacob5800/GearSetsCompanion/releases/download/v'+manifest['AssemblyVersion']+'/GearSetsCompanion.zip'
entry.update(DownloadLinkInstall=url,DownloadLinkUpdate=url,DownloadLinkTesting=url)
(p/'repo.json').write_text(json.dumps([entry],indent=2),encoding='utf-8')
print('Packaged',manifest['AssemblyVersion'])
