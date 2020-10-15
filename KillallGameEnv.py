import os
import subprocess
import logging
import platform
import gym
import socket
import GamebotAPI_pb2
from socket_function import MySocket
from contextlib import closing
from google.protobuf.json_format import MessageToDict
import time

from preprocessor import preprocessor

logging.basicConfig()


# 递归寻找num个空余端口
def find_free_port(num, port_list):
    with closing(socket.socket(socket.AF_INET, socket.SOCK_STREAM)) as s:
        s.bind(('', 0))
        s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        if (num > 1):
            find_free_port(num - 1, port_list)
        port_list.append(s.getsockname()[1])


@preprocessor
class KillallGameEnv(gym.Env):
    def __init__(self, **kwargs):
        # gym形式相关变量
        self.observation = None
        self.done = False
        self.reward = 0
        self.info = {}

        # 指定几帧交互一次
        self.frame = kwargs.get("frame", 6)
        frame_arg = "--frame {}".format(self.frame)

        # 找到可用端口
        self.process_unity = None
        self.port_list = []
        find_free_port(1, self.port_list)
        port_arg = "--port_num1 {}".format(self.port_list[0])
        type_arg = "--sevice_type {}".format("SOCKET_VS_BT")

        # start socket between train model and unity
        self.game_socket = MySocket(self.port_list[0])
        self.request = GamebotAPI_pb2.Request()
        self.response = GamebotAPI_pb2.Response()

        # start game in another process
        working_path = os.path.dirname(os.path.realpath(__file__))
        if platform.system() == 'Darwin':
            app_name = os.path.join(working_path, "Game.app/Contents/MacOS/Game")
            self.process_unity = subprocess.Popen([app_name, port_arg, type_arg])
        elif platform.system() == 'Windows':
            app_name = os.path.join(working_path, "Game/game.exe")
        elif platform.system() == 'Linux':
            app_name = os.path.join(working_path, "bin/Game.x86_64")
            self.process_unity = subprocess.Popen([app_name, port_arg, type_arg, frame_arg, "-nographics"])
        else:
            logging.error("{} not supported".format(platform.system()))
            exit()

        self.game_socket.connect()

    def reset(self):
        self.done = False
        self.reward = 0
        self.info = {}
        self.request = self.game_socket.socket_recv()
        assert self.request.WhichOneof('request') == 'restart_game', 'not reset'
        return MessageToDict(self.request, including_default_value_fields=True)

    def step(self, action):
        step_response = GamebotAPI_pb2.ResponseStep()
        # 添加动作,动作的格式为[move, shoot, mouse_x, mouse_y]
        step_response.action.move = action[0]
        step_response.action.shoot = bool(action[1])
        step_response.action.mouse_x = action[2]
        step_response.action.mouse_y = action[3]
        self.response.step.CopyFrom(step_response)

        self.game_socket.socket_send(self.response)
        self.request = self.game_socket.socket_recv()
        cnt = 0
        while self.request.WhichOneof('request') != 'step':
            time.sleep(0.01)
            cnt += 1
            self.game_socket.socket_send(self.response)
            self.request = self.game_socket.socket_recv()
            assert cnt < 100, self.request
        self.done = self.request.step.done
        return MessageToDict(self.request, including_default_value_fields=True), self.reward, self.done, self.info

    def close(self):
        # end game by sending end protocol
        self.response.quit.CopyFrom(GamebotAPI_pb2.ResponseQuit())
        self.game_socket.socket_send(self.response)
        if self.process_unity:
            self.process_unity.wait()
        self.game_socket.socket_close()


def test():
    import numpy as np
    env = KillallGameEnv(**{"frame": 6})
    s = env.reset()
    count = 0
    while True:
        s, r, done, info = env.step([np.random.randint(0, 8),
                                     np.random.randint(0, 2),
                                     np.random.randint(0, 9),
                                     np.random.randint(0, 9)])
        count += 1
        if count % 10 == 0:
            print(count)
        if done:
            break
