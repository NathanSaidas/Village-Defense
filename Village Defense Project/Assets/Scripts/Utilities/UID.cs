//Copyright (c) 2015 Nathan Hanlan
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.

using UnityEngine;
using System.Collections.Generic;

#region CHANGE LOG
/// -- April    15, 2015 - Nathan Hanlan - Added UID class.
#endregion

///<summary>
/// Represents a unique ID.
///</summary>
public struct UID 
{
    /// <summary>
    /// 
    /// </summary>
    private static List<ushort> s_FreeList = new List<ushort>();
    private static ushort s_NextNumber = 1;
    private static ushort s_LargestNumber = 1;
    public static readonly UID BAD_ID = new UID(0);

    private ushort m_ID;

    public UID(ushort aValue)
    {
        m_ID = aValue;
    }

    public static implicit operator ushort(UID aUID)
    {
        return aUID.id;
    }
    public static implicit operator short(UID aUID)
    {
        return (short)aUID.id;
    }
    public static implicit operator int(UID aUID)
    {
        return (int)aUID.id;
    }
    public static implicit operator UID(ushort aValue)
    {
        return new UID(aValue);
    }
    public static implicit operator UID(short aValue)
    {
        return new UID((ushort)aValue);
    }
    public static implicit operator UID(int aValue)
    {
        return new UID((ushort)aValue);
    }

    public static UID New()
    {
        ushort uniqueNumber = 0;

        while(s_FreeList.Count > 0)
        {
            uniqueNumber = s_FreeList[0];
            s_FreeList.RemoveAt(0);
            return new UID(uniqueNumber);
        }
        uniqueNumber = s_NextNumber;
        s_NextNumber++;
        
        if(uniqueNumber > s_LargestNumber)
        {
            s_LargestNumber = uniqueNumber;
        }
        return uniqueNumber;
    }

    public void Release()
    {
        if(m_ID == s_LargestNumber)
        {
            s_LargestNumber = 0;
        }

        if(!(m_ID >= s_NextNumber))
        {
            s_FreeList.Add(m_ID);
        }

        if(s_NextNumber > s_LargestNumber && !s_FreeList.Contains(s_NextNumber))
        {
            s_LargestNumber = s_NextNumber;
        }
        m_ID = 0;
    }

    public static void Release(UID aID)
    {
        aID.Release();
    }

    public ushort id
    {
        get { return m_ID; }
        set { m_ID = value; }
    }
	
}
