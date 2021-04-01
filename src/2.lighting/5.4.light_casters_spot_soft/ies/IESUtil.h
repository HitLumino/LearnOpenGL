//
// Created by 朱鸣鸣 on 2021/4/1.
//

#ifndef LEARNOPENGL_IESUTIL_H
#define LEARNOPENGL_IESUTIL_H


#include <string>
#include "ies_loader.h"

struct IESOutputData
{
    std::uint32_t width;
    std::uint32_t height;
    std::uint32_t channel;
    std::vector<float> stream;
};

class IESUtil {
    bool IES2HDR(const std::string& path, const std::string& outpath, IESFileInfo& info);

public:
    bool IES2HDR(const std::string& path, IESFileInfo& info);
};


#endif //LEARNOPENGL_IESUTIL_H
