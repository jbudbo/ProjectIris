docker run `
	--rm -it `
	-v ${PWD}:/usr/src `
	-w /usr/src `
	alpine `
	/bin/sh -c "apk add protoc && mkdir -p cs java && protoc --csharp_out=cs --java_out=java *.proto"