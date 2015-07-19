using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2pProxy.UPNP
{
    public enum ProtocolInfoType
    {
        Unknown, Http, Rtp, Any
    }

    /// <summary>
    /// DLNA.ORG_PS: play speed parameter (integer)
    /// </summary>
    public enum PlaySpeed
    {
        /// <summary>
        /// 0 invalid play speed
        /// </summary>
        Invalid = 0,
        /// <summary>
        /// 1 normal play speed
        /// </summary>
        Normal = 1
    }

    /// <summary>
    /// DLNA.ORG_CI: conversion indicator parameter (integer)
    /// </summary>
    public enum Conversion
    {
        /// <summary>
        /// 0 not transcoded
        /// </summary>
        None = 0,
        /// <summary>
        /// 1 transcoded
        /// </summary>
        Transcode = 1
    }

    /// <summary>
    /// DLNA.ORG_OP: operations parameter (string)
    /// </summary>
    public enum Operation
    {
        /// <summary>
        /// "00" (or "0") neither time seek range nor range supported
        /// </summary>
        None = 0x00,
        /// <summary>
        /// "01" range supported
        /// </summary>
        Range = 0x01,
        /// <summary>
        /// "10" time seek range supported
        /// </summary>
        TimeSeek = 0x10,
        /// <summary>
        /// "11" both time seek range and range supported
        /// </summary>
        Both = 0x11
    }

    /// <summary>
    /// DLNA.ORG_FLAGS, padded with 24 trailing 0s
    /// </summary>
    public enum Flag
    {
        /// <summary>
        /// 80000000  31  senderPaced
        /// </summary>
        SenderPaced = (1 << 31),
        /// <summary>
        /// 40000000  30  lsopTimeBasedSeekSupported
        /// </summary>
        TimeBasedSeek = (1 << 30),
        /// <summary>
        /// 20000000  29  lsopByteBasedSeekSupported
        /// </summary>
        ByteBasedSeek = (1 << 29),
        /// <summary>
        /// 10000000  28  playcontainerSupported
        /// </summary>
        PlayContainer = (1 << 28),
        /// <summary>
        /// 8000000  27  s0IncreasingSupported
        /// </summary>
        S0Increase = (1 << 27),
        /// <summary>
        /// 4000000  26  sNIncreasingSupported
        /// </summary>
        SNIncrease = (1 << 26),
        /// <summary>
        /// 2000000  25  rtspPauseSupported
        /// </summary>
        RtspPause = (1 << 25),
        /// <summary>
        /// 1000000  24  streamingTransferModeSupported
        /// </summary>
        StreamingTransferMode = (1 << 24),
        /// <summary>
        /// 800000  23  interactiveTransferModeSupported
        /// </summary>
        InteractiveTransferMode = (1 << 23),
        /// <summary>
        /// 400000  22  backgroundTransferModeSupported
        /// </summary>
        BackgroundTransferMode = (1 << 22),
        /// <summary>
        /// 200000  21  connectionStallingSupported
        /// </summary>
        ConnectionStall = (1 << 21),
        /// <summary>
        /// 100000  20  dlnaVersion15Supported
        /// </summary>
        DLNA_V15 = (1 << 20)
    }

    public enum MediaClass
    {
        Unknown, Image, Audio, AV, Collection
    }

    public enum MediaProfile
    {
        ImageJpeg, ImagePng, 
        AudioAc3, AudioAmr, AudioAtrac3, AudioLpcm, AudioMp3, AudioMpeg4, AudioWma,
        AvMpeg1, AvMpeg2, AvMpeg4Part2, AvMpeg4Part10, AvWmv9
    }

    public struct DlnaProfile
    {
        /// <summary>
        /// Profile ID, part of DLNA.ORG_PN= string
        /// </summary>
        public string id;
        /// <summary>
        /// Profile MIME type 
        /// </summary>
        public string mime;
        /* Profile Label */
        string label;
        /// <summary>
        /// Profile type: IMAGE / AUDIO / AV
        /// </summary>
        MediaClass mclass;
    }
}
