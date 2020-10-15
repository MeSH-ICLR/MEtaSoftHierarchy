all: GamebotAPI_pb2.py

GamebotAPI_pb2.py: GamebotAPI.proto
	protoc -I=./ --python_out=./ GamebotAPI.proto

test:
	python -m unittest discover

sdk: README.md GamebotAPI.proto GameEnv.py
	tar czf Gamebot-SDK.tar.gz $?

clean:
	rm -f GamebotAPI_pb2.py
