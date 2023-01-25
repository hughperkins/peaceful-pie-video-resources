import argparse
from dataclasses import dataclass
from typing import Any, Tuple
from numpy.typing import NDArray
import numpy as np
from peaceful_pie.unity_comms import UnityComms
from gym import Env, spaces
from stable_baselines3.common.env_checker import check_env


@dataclass
class MyVector3:
    x: float
    y: float
    z: float


@dataclass
class RlResult:
    reward: float
    finished: bool
    obs: MyVector3


class MyEnv(Env):
    def __init__(self, unity_comms: UnityComms):
        self.unity_comms = unity_comms
        self.action_space = spaces.Discrete(4)
        self.observation_space = spaces.Box(low=-np.inf, high=np.inf, shape=(3,), dtype=np.float32)

    def step(self, action: NDArray[np.uint8]) -> Tuple[NDArray[np.float32], float, bool, dict[str, Any]]:
        action_str = [
            "north",
            "south",
            "east",
            "west"
        ][action]
        rl_result: RlResult = self.unity_comms.Step(action=action_str, ResultClass=RlResult)
        info = {"finished": rl_result.finished}
        return self._obs_vec3_to_np(rl_result.obs), rl_result.reward, rl_result.finished, info

    def reset(self) -> NDArray[np.float32]:
        obs_vec3: MyVector3 = self.unity_comms.Reset(ResultClass=MyVector3)
        return self._obs_vec3_to_np(obs_vec3)

    def _obs_vec3_to_np(self, vec3: MyVector3) -> NDArray[np.float32]:
        return np.array([vec3.x, vec3.y, vec3.z], dtype=np.float32)


def run(args: argparse.Namespace) -> None:
    unity_comms = UnityComms(port=args.port)
    my_env = MyEnv(unity_comms=unity_comms)
    check_env(env=my_env)


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--port', type=int, default=9000)
    args = parser.parse_args()
    run(args)
