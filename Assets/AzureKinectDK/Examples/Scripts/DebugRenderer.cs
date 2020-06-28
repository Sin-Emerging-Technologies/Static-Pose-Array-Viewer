﻿using System;
using System.IO;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor.BodyTracking;

public class DebugRenderer : MonoBehaviour
{
    Skeleton skeleton;
    GameObject[] blockman;
    public GameObject blockmanParent;
    // public Renderer renderer;
    string[] filePaths;
    int currentFileIndex = 0;
    string[] _currentFile;
    int currentSkeletonIndex = 1;
    int skeletonIndexToLoad = 1;
    string[] currentPoseCoords;
    string[] poseLabels;
    public Text text_currentSkeletonIndex;
    float x;

    void Start(){
        Debug.Log("start");
        blockman = makeBlockman();
        // blockman.parent = blockmanParent.transform;
        // blockmanParent.transform.Rotate(0.0f, 90.0f, 0.0f);
        // blockmanParent.transform.RotateAround(transform.position, transform.up, Time.deltaTime * 90f);
        // blockmanParent.transform.rotation = Quaternion.Euler(90,0,0);
        filePaths = getPoseFiles();
        loadFile(currentFileIndex);
    }

    int loadFile(int fileIndex){
        if (filePaths == null || filePaths.Length < 1)
        {
            Debug.Log("filePaths array is null or empty");
            return -1;
        }
        if (fileIndex >= filePaths.Length)
        {
            Debug.Log("reached end of file list");
            return -1;
        }

        string currentFilePath = filePaths[fileIndex];
        Debug.Log(currentFilePath);
        _currentFile = System.IO.File.ReadAllLines(@currentFilePath);
        poseLabels = new string[_currentFile.Length];
        // Debug.Log(lines[0]); // headers
        
        if (_currentFile == null || _currentFile.Length < 2) // header + data row
        {
            Debug.Log("currentFile array is null or empty");
            return -1;
        }

        currentPoseCoords = _currentFile[1].Split(',');

        return 0;
    }
    int loadSkeleton(int idxToLoad){
        if (_currentFile == null || _currentFile.Length < 2) // header + data row
        {
            Debug.Log("currentFile array is null or empty");
            return -1;
        }

        if (idxToLoad < _currentFile.Length)
        {
            currentPoseCoords = _currentFile[idxToLoad].Split(',');
            return 0;
        }

        if (filePaths == null || filePaths.Length < 2) // header + data row
        {
            Debug.Log("filePaths array is null or empty");
            return -1;
        }

        Debug.Log("load next file");

        return 0;
    }

    void Update(){
        if (currentSkeletonIndex != skeletonIndexToLoad)
        {
            loadSkeleton(skeletonIndexToLoad);
            currentSkeletonIndex = skeletonIndexToLoad;
        }
        text_currentSkeletonIndex.text = "currentSkeletonIndex: " + currentSkeletonIndex;
        updateBlockman(currentPoseCoords);

        
        // x += Time.deltaTime * 10;
        // blockmanParent.transform.rotation = Quaternion.Euler(x,0,0);
    }

    public void loadNextSkeleton(){
        if(skeletonIndexToLoad < _currentFile.Length) {
            skeletonIndexToLoad = currentSkeletonIndex + 1;
            Debug.Log("loadNextSkeleton");
        } else {
            Debug.Log("loadNextSkeleton reached limit");
        }
    }

    public void loadPrevSkeleton(){
        if(skeletonIndexToLoad > 0) {
            skeletonIndexToLoad = currentSkeletonIndex - 1;
            Debug.Log("loadPrevSkeleton");
        } else {
            Debug.Log("loadPrevSkeleton reached limit");
        }
    }

    private string[] labelCurrentSkeleton(string[] labels, int index, string label) {
        string[] labelsCopy = (string[]) labels.Clone(); // shallow copy
        if (labelsCopy != null && index < labelsCopy.Length) {
            labelsCopy[index] = label;
            Debug.Log("markAs_" + label);
            loadNextSkeleton();
        } else {
            Debug.Log("markAs_" + label + " index error");
        }
        return labelsCopy;
    }
    public void markAsStanding(){
        poseLabels = labelCurrentSkeleton(poseLabels, currentSkeletonIndex, "standing,");
    }

    public void markAsSquatDown(){
        poseLabels = labelCurrentSkeleton(poseLabels, currentSkeletonIndex, "squatDown,");
    }

    public void markAsSquatUp(){
        poseLabels = labelCurrentSkeleton(poseLabels, currentSkeletonIndex, "squatUp,");
    }

    public void markAsSquatBottom(){
        poseLabels = labelCurrentSkeleton(poseLabels, currentSkeletonIndex, "squatBottom,");
    }

    public void writePoseArrayToFile(){
        string currFilePath = filePaths[currentFileIndex];
        string folder = @"squat-front-100-dan-csv\";
        string[] parts = currFilePath.Split(new string[] { folder }, StringSplitOptions.None);
        string newFilePath = parts[0] + @"squat-front-100-dan-csv\labelledPoses\" + parts[1];
        using(StreamWriter writetext = new StreamWriter(@"D:\Downloads\squat-front-100-dan-csv")){
            for(int i=0;i<_currentFile.Length;i++) {
                writetext.WriteLine(_currentFile[i] + poseLabels[i]);
            }
        }
        Debug.Log("Wrote to " + newFilePath);
    }

    void updateBlockman(string[] skeletonString){
        for (var i = 0; i < (int)JointId.Count; i++)
        {
            var x = float.Parse(skeletonString[i*3 + 0]);
            var y = float.Parse(skeletonString[i*3 + 1]);
            var z = float.Parse(skeletonString[i*3 + 2]);

            var v = new Vector3(x, -y, z) * 0.004f;
            blockman[i].transform.position = v;
        }
    }

    string[] getPoseFiles(){
        Debug.Log("get files");
        DirectoryInfo d = new DirectoryInfo(@"D:\Downloads\squat-front-100-dan-csv");
        FileInfo[] Files = d.GetFiles("*.txt");
        // Debug.Log(Files[0].Directory); // D:\Downloads\squat-front-100-dan-csv instance of folder
        // Debug.Log(Files[0].DirectoryName); // D:\Downloads\squat-front-100-dan-csv string
        // Debug.Log(Files[0].Name); // 6112020 101909 PM.txt file name
        // Debug.Log(Files[0].FullName); // D:\Downloads\squat-front-100-dan-csv\6112020 101909 PM.txt full path of dir or file

        string[] filePaths = new string[Files.Length];
        for (int i=0;i<Files.Length;i++) // foreach(FileInfo file in Files )
        {
            filePaths[i] = Files[i].FullName;
        }
        
        return filePaths;
    }

    GameObject[] makeBlockman(){
        GameObject[] debugObjects = new GameObject[(int)JointId.Count];
        for (var i = 0; i < (int)JointId.Count; i++)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = Enum.GetName(typeof(JointId), i);
            cube.transform.localScale = Vector3.one * 0.4f;
            cube.transform.parent = blockmanParent.transform;
            debugObjects[i] = cube;
        }
        return debugObjects;
    }

}