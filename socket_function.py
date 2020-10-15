import socket
import GamebotAPI_pb2

class MySocket:
    def __init__(self, port, buf_size = 10240):
        self.port = port
        self.env_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        # 设置缓存区大小，需要和游戏侧同步
        self.buf_size = buf_size
        self.env_socket.setsockopt(
            socket.SOL_SOCKET,
            socket.SO_SNDBUF,
            self.buf_size
        )
        self.env_socket.setsockopt(
            socket.SOL_SOCKET,
            socket.SO_RCVBUF,
            self.buf_size
        )
        # 训练侧为服务端
        try:
            self.env_socket.bind(("127.0.0.1", self.port))
            self.env_socket.listen(1)
        except:
            print("server not build")

    def connect(self):
        self.conn, self.address = self.env_socket.accept()  # 等待连接，此处自动阻塞

    def socket_send(self, response):
        # 发送请求
        response_buf = b''
        response_data = response.SerializeToString()
        response_buf += (len(response_data)).to_bytes(4, byteorder='big')
        response_buf += response_data
        self.conn.sendall(response_buf)

    def socket_recv(self):
        # 包的格式：len+data。len为4字节int，big-endian；data为protobuf数据内容
        # 包的格式：len+data。len为4字节int，little-endian；data为protobuf数据内容
        len_buf = b''
        request_buf = b''
        received_len = 0
        while received_len < 4:
            first_buf = self.conn.recv(4 - received_len)
            len_buf += first_buf
            received_len += len(first_buf)
        assert len(len_buf) == 4, 'len_buf should be 4'
        file_size = int.from_bytes(len_buf, byteorder='big', signed=True)
        received_len = 0
        while received_len < file_size:
            second_buf = self.conn.recv(file_size - received_len)
            request_buf += second_buf
            received_len += len(second_buf)
        assert received_len == file_size, 'receive {} should equal to define {}'.format(received_len, file_size)

        tmp_request = GamebotAPI_pb2.Request()
        tmp_request.ParseFromString(request_buf)
        return tmp_request

    def socket_close(self):
        self.conn.close()
        self.env_socket.close()
