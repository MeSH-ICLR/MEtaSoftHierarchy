#include <pybind11/pybind11.h>
#include <pybind11/numpy.h>
#include "GamebotAPI.pb.h"
#include <pybind11/stl.h>
#include <vector>


class GamebotCppEnv {
public:
    GamebotCppEnv() {
        obs_shape.push_back(2);
        obs_shape.push_back(3);
        act_shape.push_back(2);
        act_shape.push_back(4);
        state_win_user_index = 0;
    }
    std::vector<int> get_obs_shape() {
        return obs_shape;
    }
    std::vector<int> get_act_shape() {
        return act_shape;
    }
    void reset(const std::string &input) {
    }
    void step(const std::string &input) {
        if (req_step.ParseFromString(input)) {
            state_win_user_index = req_step.state_win_user_index();
        }
    }
    std::vector<int> get_obs() {
      std::vector<int> output {1, 2, 3, 4, 5, 6};
      return output;
    }
    int get_reward() {
        if (state_win_user_index == 1) {
            return 1;
        } else if (state_win_user_index == 2) {
            return -1;
        } else {
            return 0;
        }
    }
    std::vector<int> get_info() {
    }

private:
    std::vector<int> obs_shape;
    std::vector<int> act_shape;
    int state_win_user_index;
    int reward;
    GamebotAPIProtocol::RequestStep req_step;
};


namespace py = pybind11;

PYBIND11_MODULE(gamebot_bind, m) {
    m.doc() = "Gamebot binding";

    py::class_<GamebotCppEnv>(m, "GamebotCppEnv")
        .def(py::init<>())
        .def("get_obs_shape", &GamebotCppEnv::get_obs_shape)
        .def("get_act_shape", &GamebotCppEnv::get_act_shape)
        .def("reset", &GamebotCppEnv::reset)
        .def("step", &GamebotCppEnv::step)
        .def("get_obs", &GamebotCppEnv::get_obs)
        .def("get_reward", &GamebotCppEnv::get_reward)
        .def("get_info", &GamebotCppEnv::get_info);

#ifdef VERSION_INFO
    m.attr("__version__") = VERSION_INFO;
#else
    m.attr("__version__") = "dev";
#endif
}
