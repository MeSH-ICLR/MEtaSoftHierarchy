from KillallGameEnv import KillallGameEnv
import unittest
import time

class TestGameEnv():
    def __init__(self):
        self.env_config = {"frame":6}
        self.env = KillallGameEnv(**self.env_config)

    def test_episode(self):
        for i_episode in range(2):
            observation = self.env.reset()
            while True:
                t = time.time()
                observation, reward1, reward2, done, info = self.env.step([1,1,0,0])
                print(time.time() - t)
                if done:
                    break
        self.env.close()


if __name__ == '__main__':
    t = TestGameEnv()
    t.test_episode()

