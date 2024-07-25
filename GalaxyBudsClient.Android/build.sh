#!/bin/sh
if [[ $1 = "aab" ]] || [[ $1 = "apk" ]] && [[ -n $2 ]] ; then :
else
  echo "Usage: "$0" [aab|apk] [additional_prop]"
  exit 1
fi

if [[ -z $PASS ]]; then
  MY_SCRIPT_VARIABLE="Env var PASS to decrypt keystore not set"
fi

dotnet publish -f net8.0-android -c Debug -p:AndroidKeyStore=true -p:AndroidSigningKeyStore=/home/tim/Development/_KeyStores/keystore.jks -p:AndroidPackageFormats=$1 -p:AndroidSigningKeyAlias=galaxybudsclient -p:AndroidSdkDirectory=/home/tim/Android/Sdk -p:AndroidSigningKeyPass="env:PASS" -p:AndroidSigningStorePass="env:PASS" -p:DebugType=PdbOnly -p:NotDebuggable=true -p:EmbedAssembliesIntoApk=true -p:$2 -v d
