﻿#include <iostream>
#include <string>
#include <vector>
#include "..\BearBundle\BearCore\BearCore.hpp"
#include "..\BearBundle\BearGraphics\BearGraphics.hpp"

#if !defined(_DEBUG) && defined(_WIN64) 
#include "tbb/parallel_for.h" 
#include "tbb/blocked_range.h"
#endif

#include "Utils.hpp"

int iReaderPos = 0;

using namespace std;

struct Textures
{
    string main;
    string temp;
};

vector<Textures> LoadObJTexturesVector(char** args, int count)
{
    vector<Textures> vec;

    for (int i = 0; i < count; i++)
    {
        Textures texture;
        texture.main = args[iReaderPos]; iReaderPos++;
        texture.temp = args[iReaderPos]; iReaderPos++;
        vec.push_back(texture);
    }

    return vec;
}

bool HasAlpha(BearImage& Texture)
{
    bool alpha = false;
    FOR_START(size_t, 0, Texture.GetWidth(), w)
    {
        for (size_t h = 0; h < Texture.GetHeight(); h++)
        {
            uint8 px = Texture.GetPixel(w, h).A8U;
            if (px < 254)
                alpha = true;

            if (alpha)
            {
                #if !defined(_DEBUG) && defined(_WIN64) 
                    tbb::task::self().cancel_group_execution();
                #endif
                break;
            }
        }
        if (alpha)
            break;
    }
    FOR_END

    return alpha;
}

void ConvertDDStoPng(string dds, string png, bool alpha)
{
	BearImage Texture;

    std::wstring widestr = std::wstring(dds.begin(), dds.end());
    const wchar_t* widecstr = widestr.c_str();

    Texture.LoadFromFile(widecstr);
    Texture.Convert(alpha && HasAlpha(Texture) ? BearTexturePixelFormat::R8G8B8A8 : BearTexturePixelFormat::R8G8B8);

    widestr = std::wstring(png.begin(), png.end());
    widecstr = widestr.c_str();
    Texture.SaveToPng(widecstr);
}

int main(int argc, char** argv)
{
    BearCore::Initialize();

    bool use_alpha = false;
    int textures_cnt = 0;

    use_alpha = !!atoi(argv[0]); iReaderPos++;
    textures_cnt = atoi(argv[1]); iReaderPos++;

    vector<Textures> pTextures = LoadObJTexturesVector(argv, textures_cnt);

    if (use_alpha || pTextures.size() < 2)
    {
        for (int i = 0; i < pTextures.size(); i++)
            ConvertDDStoPng(pTextures[i].main, pTextures[i].temp, use_alpha);
    }
    else
    {
        FOR_START(int, 0, pTextures.size(), i)
        {
            ConvertDDStoPng(pTextures[i].main, pTextures[i].temp, false);
        }
        FOR_END
    }

    BearCore::Destroy();
}