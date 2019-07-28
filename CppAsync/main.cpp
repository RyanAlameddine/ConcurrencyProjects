#include <uv.h>
#include <stdint.h>
#include <iostream>
#include <functional>
#include <mutex>

#include <future>
#include <vector>
#include <queue>

#include <Windows.h>

static void Allocate(uv_handle_t* handle, size_t suggestedSize, uv_buf_t* buffer) {
	buffer->base = (char*) malloc(suggestedSize);
	buffer->len = suggestedSize;
}

static void CloseHandleUv(uv_handle_t* handle, void* param) {
	free(handle);
}

static void LoopWalk(uv_handle_t* handle, void* param) {
	uv_close(handle, (uv_close_cb) CloseHandleUv);
	printf("Walking");
}

static void TTYRead(uv_stream_t* stream, ssize_t length, const uv_buf_t* buffer) {
	printf("Recieved buffer\n");

	char data[256];

	int copyLength = length < 256 ? length : 255;

	memcpy(data, buffer->base, copyLength);

	data[copyLength] = '\0';

	printf("%s", data);

	if (strcmp(data, "QUIT\r\n") == 0) {
		printf("Quitting\n");
		uv_walk(stream->loop, LoopWalk, NULL);
	}

	free(buffer->base);
}

static uv_async_t* asyncHandle;

static DWORD PASCAL ThreadMain(LPVOID lpThreadParameter) {
	printf("Hello from the thread\n");
	uv_loop_t* loop = (uv_loop_t*) lpThreadParameter;

	uv_tty_t* tty = (uv_tty_t*) malloc(sizeof(uv_tty_t));
	uv_tty_init(loop, tty, 0, TRUE);

	uv_read_start((uv_stream_t*)tty, Allocate, TTYRead);

	uv_run(loop, UV_RUN_DEFAULT);

	return 0;
}

template <typename T>
struct AsyncFunction {
	AsyncFunction(uv_loop_t* loop, std::function<void(std::promise<T>&)> callback) : callback { std::move(callback) }, loop { loop } {
		async = (uv_async_t*) malloc(sizeof(uv_async_t));
		uv_async_init(loop, async, [](uv_async_t* handle) {
			auto& self = *(AsyncFunction<T>*) handle->data;
			std::lock_guard<std::mutex> l{ self.lock };
			while (!self.calledFunctions.empty()) {
				auto& promise = self.calledFunctions.front();
				self.callback(promise);
				self.calledFunctions.pop();
			}
		});
		if (async != nullptr) {
			async->data = this;
		}
	}

	std::future<T> CallFunction() {

		if (std::this_thread::get_id() == *(std::thread::id*)loop->data) {
			std::promise<T> promise;
			callback(promise);
			return promise.get_future();
		}

		std::future<T> future;
		{
			std::lock_guard<std::mutex> l{ lock };
			calledFunctions.emplace();
			future = calledFunctions.back().get_future();
		}
		uv_async_send(async);
		return future;
	}

	std::queue<std::promise<T>> calledFunctions;

	std::function<void(std::promise<T>&)> callback;
	std::mutex lock;

	uv_async_t* async;
	uv_loop_t* loop;
};

int main() {

	uv_loop_t* loop = (uv_loop_t*) malloc(sizeof(uv_loop_t));
	uv_loop_init(loop);

	loop->data = new std::thread::id(std::this_thread::get_id());

	AsyncFunction<int> async{ loop, [](std::promise<int>& promise) {
		promise.set_value(52);
	} };

	DWORD threadId = 0;
	HANDLE threadHandle = CreateThread(NULL, 0, ThreadMain, loop, 0, &threadId);

	auto future = async.CallFunction();

	if (future.wait_for(std::chrono::milliseconds(0)) != std::future_status::timeout) {
		//TRUE if it has a result, false otherwise
	}

	future.wait();

	std::cout << future.get() << std::endl;

	if (threadHandle != 0) {
		WaitForSingleObject(threadHandle, INFINITE);
	}

	free(loop);
}