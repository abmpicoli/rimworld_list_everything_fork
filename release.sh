#!/bin/bash -x
set -e
cd "$(readlink -f $(dirname "$0"))"
RELEASE_VERSION="$(readlink -f "../$(basename $PWD)_RELEASE")"
export RELEASE_VERSION
case "$1" in 
	DEV)
		cp "$RELEASE_VERSION/About/PublishedFileId.txt" About || echo 'Release not uploaded to steam yet'
		rm -R "$RELEASE_VERSION" || echo 'RELEASE VERSION not released yet'
		mv About/About.xml.off About/About.xml || echo 'It seems the DEV version is active. All ok'
	;;
	RELEASE)
		rm -R "$RELEASE_VERSION" || echo "$RELEASE_VERSION deleted"
		mkdir "$RELEASE_VERSION" || echo "$RELEASE_VERSION already created"
		mkdir "$RELEASE_VERSION/About" || echo 'about folder created ok'
		cp About/Preview.png "$RELEASE_VERSION/About"
		cp -R Assemblies "$RELEASE_VERSION/Assemblies"
		cp -R Defs "$RELEASE_VERSION/Defs"
		cp -R Languages "$RELEASE_VERSION/Languages"
		
		find Textures -type f -name '*.png' | xargs ./copy_to_release.sh
		cp About/About.xml "$RELEASE_VERSION/About" || { echo 'About.xml is not available' ; exit 1 ;  }
		mv About/About.xml About/About.xml.off
	
	;;
	SUBSCRIBED) 
		mv About/About.xml About/About.xml.off|| echo 'Dev version already turned off. All ok'
		mv "$RELEASE_VERSION/About/About.xml" "$RELEASE_VERSION/About/About.xml.off" || echo 'Release local version already turned off. All ok'
	;;
	*) echo 'use either DEV or RELEASE or SUBSCRIBED'
		exit 1
	;;
esac
