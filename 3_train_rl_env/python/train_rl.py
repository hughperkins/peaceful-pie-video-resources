import argparse
from dataclasses import dataclass
from peaceful_pie.unity_comms import UnityComms
from stable_baselines3.ppo.ppo import PPO
from stable_baselines3.common.monitor import Monitor
from my_env import MyEnv


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


def run(args: argparse.Namespace) -> None:
    unity_comms = UnityComms(port=args.port)
    my_env = MyEnv(unity_comms=unity_comms)
    my_env = Monitor(my_env)
    ppo = PPO("MlpPolicy", env=my_env, verbose=1, ent_coef=0.1)
    ppo.learn(total_timesteps=1000000)


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--port', type=int, default=9000)
    args = parser.parse_args()
    run(args)
