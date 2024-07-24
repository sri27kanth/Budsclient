#!/bin/sh
dotnet publish -f net8.0-android -c Debug -p:AndroidKeyStore=true -p:AndroidSigningKeyStore=/home/tim/Development/_KeyStores/keystore.jks -p:AndroidPackageFormats=aab -p:AndroidSigningKeyAlias=galaxybudsclient -p:AndroidSdkDirectory=/home/tim/Android/Sdk -p:AndroidSigningKeyPass="file:/home/tim/.keys/jks" -p:AndroidSigningStorePass="file:/home/tim/.keys/jks" -p:DebugType=PdbOnly -p:NotDebuggable=true
