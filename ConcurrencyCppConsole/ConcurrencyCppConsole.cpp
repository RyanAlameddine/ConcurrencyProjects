#include <thread>
#include <atomic>
#include <mutex>
#include <iostream>
#include <condition_variable>

std::mutex mutex;
std::condition_variable cond;
static int incoming = -2;

void threadMain(const char* str, int c) {
	std::unique_lock<std::mutex> m{ mutex };
	while (true) {
		if (incoming == c - 1) {
			incoming = c;
			break;
		}

		cond.wait(m);
	}

	std::cout << str << std::endl;
	cond.notify_all();
}

int main()
{
	std::thread thr1{ threadMain, "Test",  0 };
	std::thread thr2{ threadMain, "wowwww", 1 };
	std::thread thr3{ threadMain, "cvbxcv", 2 };

	incoming = -1;

	thr1.join();
	thr2.join();
	thr3.join();
	for(;;){}
}


