//
// Created by 朱鸣鸣 on 2021/4/1.
//

#include <ios>
#include <fstream>
#include <iostream>
#include "IESUtil.h"
#include "rgbe.h"

bool IESUtil::IES2HDR(const std::string &path, const std::string &outpath, IESFileInfo &info) {
    std::fstream stream(path, std::ios_base::binary | std::ios_base::in);
    if (!stream.is_open())
    {
        std::cout << "IES2HDR Error: Failed to open file :" << path << std::endl;
        return false;
    }

    stream.seekg(0, std::ios_base::end);
    std::streamsize streamSize = stream.tellg();
    stream.seekg(0, std::ios_base::beg);

    std::vector<std::uint8_t> IESBuffer;
    IESBuffer.resize(streamSize + 1);
    stream.read((char*)IESBuffer.data(), IESBuffer.size());

    IESLoadHelper IESLoader;
    if (!IESLoader.load((char*)IESBuffer.data(), streamSize, info))
        return false;

    IESOutputData HDRdata;
    HDRdata.width = 256;
    HDRdata.height = 256;
    HDRdata.channel = 3;
    HDRdata.stream.resize(HDRdata.width * HDRdata.height * HDRdata.channel);

    if (!IESLoader.saveAs2D(info, HDRdata.stream.data(), HDRdata.width, HDRdata.height, HDRdata.channel))
        return false;

    FILE* fp = std::fopen(outpath.c_str(), "wb");
    if (!fp)
    {
        std::cout << "IES2HDR Error: Failed to create file : " << outpath << path << std::endl;;
        return false;
    }

    rgbe_header_info hdr;
    hdr.valid = true;
    hdr.gamma = 1.0;
    hdr.exposure = 1.0;
    std::memcpy(hdr.programtype, "RADIANCE", 9);

    RGBE_WriteHeader(fp, HDRdata.width, HDRdata.height, &hdr);
    RGBE_WritePixels_RLE(fp, HDRdata.stream.data(), HDRdata.width, HDRdata.height);
    std::fclose(fp);

    return true;
}

bool IESUtil::IES2HDR(const std::string &path, IESFileInfo &info) {
    std::string outpath;
    outpath = path.substr(0, path.length() - 3);
    outpath += "hdr";

    return IES2HDR(path, outpath, info);
}
