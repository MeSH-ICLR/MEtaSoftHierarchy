cmake_minimum_required(VERSION 3.5)
project(gamebot_bind)

# find the protobuf compiler and libraries
find_package(Protobuf REQUIRED)
# check if protobuf was found
if(PROTOBUF_FOUND)
    message ("protobuf found")
else()
    message (FATAL_ERROR "Cannot find Protobuf")
endif()
# Generate the .h and .cxx files
PROTOBUF_GENERATE_CPP(PROTO_SRCS PROTO_HDRS GamebotAPI.proto)
# Print path to generated files
message ("PROTO_SRCS = ${PROTO_SRCS}")
message ("PROTO_HDRS = ${PROTO_HDRS}")

add_subdirectory(pybind11)
pybind11_add_module(gamebot_bind
    cpp/preprocessor.cc
    ${PROTO_SRCS}
    ${PROTO_HDRS})
target_include_directories(gamebot_bind
    PUBLIC
    ${PROTOBUF_INCLUDE_DIRS}
    ${CMAKE_CURRENT_BINARY_DIR}
)
target_link_libraries(gamebot_bind
    PUBLIC
    ${PROTOBUF_LIBRARIES}
)
