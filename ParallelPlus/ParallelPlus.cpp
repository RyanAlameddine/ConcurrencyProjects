// ParallelPlus.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>

#include <random>
#include <vector>
#include <execution>
#include <algorithm>
#include <chrono>

int main()
{
	std::default_random_engine generator;

	std::uniform_int_distribution<int> distribution(0, 500);

	const int length = 1000000;

	std::vector<int> items;
	items.reserve(length);
	std::vector<int> items2;
	items2.reserve(length);
	std::vector<int> items3;
	items3.reserve(length);

	for (int i = 0; i < length; i++) {
		int gen = distribution(generator);
		items.push_back(gen);
		items2.push_back(gen);
		items3.push_back(gen);
	}

	auto startTime = std::chrono::steady_clock::now();

	std::sort(items.begin(), items.end());

	std::chrono::duration<double> endStatndardSortDelta = std::chrono::steady_clock::now() - startTime;



	startTime = std::chrono::steady_clock::now();

	std::sort(std::execution::par, items2.begin(), items2.end());

	std::chrono::duration<double> endParallelSeqSortDelta = std::chrono::steady_clock::now() - startTime;



	startTime = std::chrono::steady_clock::now();

	std::sort(std::execution::par_unseq, items3.begin(), items3.end());

	std::chrono::duration<double> endParallelSortDelta = std::chrono::steady_clock::now() - startTime;





	std::cout << "standard " << endStatndardSortDelta.count() << std::endl;
	std::cout << "parallelSeq " << endParallelSeqSortDelta.count() << std::endl;
	std::cout << "parallelUsq " << endParallelSortDelta.count() << std::endl;

    std::cout << "Hello World!\n";
}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
