#!/bin/bash
[ -n "$RELEASE_VERSION" ] || \
	{ 
		echo 'this script must be called from the release.sh script. RELEASE_VERSION not defined' ; exit 1 ;
	}
for F in "$@" ; do 
	mkdir -p "$RELEASE_VERSION/$(dirname $F)"
	cp "$F" "$RELEASE_VERSION/$F"

done
