#pragma once

#include <queue>
#include <thread>
#include <atomic>
#include <mutex>
#include <iostream>
#include <condition_variable>

template<typename T>
class BlockingQueue {
private:
	std::mutex mutex;
	std::condition_variable cond;
	std::queue<T> pi;

public:
	void Enqueue(T value) {
		std::lock_guard<std::mutex> m{ mutex };
		pi.push(value);
		cond.notify_one();
	}

	bool IsEmpty() {
		std::lock_guard<std::mutex> m{ mutex };
		return pi.empty;
	}

	T Dequeue() {
		std::unique_lock<std::mutex> m{ mutex };
		while (pi.empty) {
			cond.wait(m);
		}
		T val = pi.front();
		pi.pop();
		return val;
	}
};