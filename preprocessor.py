# coding: utf-8

import copy
import math
import numpy as np


def preprocessor(cls):
    class KillallGameEnv(cls):
        def __init__(self, **kwargs):
            super(KillallGameEnv, self).__init__(**kwargs)
            self.kill = 0

        def reset(self):
            s = super().reset()
            self.has_been_to = None
            self.kill = None
            s, rw, rb, done, info = self.step([0, 0, 0, 0])
            return s

        def step(self, a):
            action = self._get_action(a)
            s, r, done, info = super().step(action)
            s = s["step"]

            done = s["done"]
            hero = s["hero"]

            hp = hero["hp"]
            energy = hero["energy"]
            x = hero["x"]
            y = hero["y"]
            z = hero["z"]
            rotationX = hero["rotationX"]
            rotationY = hero["rotationY"]
            rotationZ = hero["rotationZ"]
            kill = hero["kill"]

            s_vec = []
            s_vec += self._binary(hp, 0, 100, 64)
            s_vec += self._binary(energy, 0, 16, 16)
            s_vec += self._binary(kill, 0, 7, 8)
            s_vec = np.array(s_vec, np.float32)
            rotationX_vec = np.cos(np.linspace(0, np.pi * 2, 12, False) - rotationX / np.pi)
            rotationY_vec = np.cos(np.linspace(0, np.pi * 2, 12, False) - rotationY / np.pi)
            s_vec = np.concatenate([s_vec, rotationX_vec, rotationY_vec], axis=0)

            rayCast = hero["rayCast"]
            rayCast = np.reshape(rayCast, (25, 37, 2))

            obj = np.array(rayCast[:, :, 0], np.int32)
            obj_onehot = np.array(np.eye(5)[obj], np.uint8)

            dist = rayCast[:, :, 1]
            dist_bin = []
            bin_list = [0.25, 0.5, 1.0, 2.0, 4.0, 8.0, 16.0]
            for cut in bin_list[::-1]:
                larger_than_cut = np.array(dist > cut, np.float32)
                dist_bin.append(np.array(np.expand_dims(
                    larger_than_cut, axis=-1), np.uint8))
                dist = dist - cut * larger_than_cut
            dist_bin = np.concatenate(dist_bin, axis=-1)

            s_image = np.concatenate([obj_onehot, dist_bin], axis=-1)

            rw = 0.01
            if self.has_been_to is None:
                self.has_been_to = [y, y]
            elif y > self.has_been_to[1]:
                rw += 2.0 * (y - self.has_been_to[1])
                self.has_been_to[1] = y

            if self.kill is None:
                rb = 0
                self.kill = kill
            else:
                rb = (kill - self.kill) * 10.0
                self.kill = kill

            rw = float(rw)
            rb = float(rb)

            return {"s_image": s_image, "s_vec": s_vec}, rw, rb, done, info

        @staticmethod
        def _get_action(a):
            action = []
            action.append(a[0])
            action.append(a[1])
            if a[2] == 0:
                action.append(a[2])
            else:
                d = {k: v for k, v in zip(
                    [t for t in range(1, 9)],
                    np.linspace(-0.05, 0.05, 8))}
                action.append(d[a[2]])
            if a[3] == 0:
                action.append(a[3])
            else:
                d = {k: v for k, v in zip(
                    [t for t in range(1, 9)],
                    np.linspace(-0.05, 0.05, 8))}
                action.append(d[a[3]])
            return action

        @staticmethod
        def _binary(x, lower_bound, upper_bound, acc):
            x_proportion = (x - lower_bound) / (upper_bound - lower_bound)
            x_proportion = max(min(x_proportion, 1 - 1e-8), 0)
            length = math.ceil(np.log2(acc))
            binary_p = bin(int(x_proportion * acc))[2:]
            binary_p = "0" * (length - len(binary_p)) + binary_p
            return list(map(int, binary_p))

    return KillallGameEnv
