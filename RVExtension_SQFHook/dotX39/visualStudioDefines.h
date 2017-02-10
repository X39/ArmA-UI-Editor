#pragma once
#ifndef NOEXCEPT
	#ifdef _MSC_VER
		#define NOEXCEPT(FLAG)
	#else
		#define NOEXCEPT(FLAG) noexcept(flag)
	#endif
#endif