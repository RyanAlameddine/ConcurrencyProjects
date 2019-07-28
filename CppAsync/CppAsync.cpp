/*#include <iostream>

#include <uv.h>
#include <stdlib.h>

#include <future>
#include <thread>

int main()
{
	// Future -> Gives result in future
	// Promise -> Provides future, promises to give result

	//std::thread loopThread([] {
	//	uv_loop_t* loop = malloc(sizeof(uv_loop_t));
	//});

	std::future<int> future = std::async(std::launch::async, [] {
		return 42;
	});
	std::future<int> future2 = std::async(std::launch::async, [] {
		return 52;
	});

	future.wait();
	future2.wait();
	int res = future.get();

	std::promise<int> promise;
	auto future = promise.get_future();

	std::thread thr([](std::promise<int> p) {
		std::this_thread::sleep_for(std::chrono::milliseconds(500));
		p.set_value(78);
	}, std::move(promise));

	future.wait();

	std::cout << future.get() << std::endl;

	thr.join();
	
    std::cout << "Hello World!\n";

	return 0;
}
*/