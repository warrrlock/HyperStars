using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LSystemTree : MonoBehaviour
{
    [Serializable]
    public struct LSystemRule
    {
        public char source;
        public string target;
    }

    [Header("Generation")]
    public float angle = 25f;
    public float length = 1f;
    public float lengthDecay = 0.67f;
    [Min(0.01f)] public float radius = 0.25f;
    public float radiusDecay = 0.67f;
    [Range(1, 8)] public int iterations = 3;
    public int randomSeed = 12345;

    [Header("Mesh")]
    [Range(1, 32)] public int radialSegments = 6;
    [Range(1, 10)] public int heightSegments = 1;

    [Header("L-System")]
    public string axiom = "F";
    public List<LSystemRule> rules = new List<LSystemRule>();

    [Header("Steve's Pre-builts")]
    public string[] prebuiltLSystemRules;

    [Header("Debug")]
    public bool drawBranchLines = true;
    public bool drawLeafNodes = true;
    
    private List<Branch> branches = new List<Branch>();

    char[] GenerateSentence(char[] sentence, int iterationCount = 0)
    {
        List<char> nextSentence = new List<char>();
        for (int i = 0; i < sentence.Length; i++)
        {
            char currentChar = sentence[i];
            bool ruleApplied = false;

            foreach (LSystemRule rule in rules)
            {
                if (currentChar == rule.source)
                {
                    ruleApplied = true;
                    nextSentence.AddRange(rule.target.ToCharArray());
                    break;
                }
            }

            if (!ruleApplied)
            {
                nextSentence.Add(currentChar);
            }
        }

        sentence = nextSentence.ToArray();

        iterationCount++;
        if (iterationCount < iterations)
        {
            return GenerateSentence(sentence, iterationCount);
        }
        else
        {
            return sentence;
        }
    }

    void GenerateFoliage(Mesh mesh = null)
    {
        UnityEngine.Random.InitState(randomSeed);

        char[] sentence = GenerateSentence(axiom.ToCharArray());

        List<Branch> branchStateStack = new List<Branch>();
        branchStateStack.Add(new Branch(Matrix4x4.identity, length, radius, radius * radiusDecay, radialSegments));
        Branch workingBranchState = branchStateStack[0];
        
        branches.Clear();
        List<CombineInstance> leafMeshInstances = new List<CombineInstance>();
        List<CombineInstance> branchMeshInstances = new List<CombineInstance>();

        void AddBranch()
        {
            branches.Add(workingBranchState);

            workingBranchState.bRadius = workingBranchState.tRadius;
            workingBranchState.tRadius *= radiusDecay;
        }

        for (int i = 0; i < sentence.Length; i++)
        {
            char currentChar = sentence[i];

            if (currentChar == 'B') // Add a branch
            {
                AddBranch();
            }
            else if (currentChar == 'F') // Move forward and add branch
            {
                workingBranchState.transform *= Matrix4x4.Translate(Vector3.up * workingBranchState.length);
                AddBranch();
            }
            else if (currentChar == 't') // Move branch origin downwards
            {
                workingBranchState.transform *= Matrix4x4.Translate(Vector3.down * workingBranchState.length);
            }
            else if (currentChar == 'T') // Move branch origin upwards
            {
                workingBranchState.transform *= Matrix4x4.Translate(Vector3.up * workingBranchState.length);
            }
            else if (currentChar == 'x') // Rotate ccw around X axis
            {
                workingBranchState.transform *= Matrix4x4.Rotate(Quaternion.Euler(+angle, 0, 0));
            }
            else if (currentChar == 'X') // Rotate cw around X axis
            {
                workingBranchState.transform *= Matrix4x4.Rotate(Quaternion.Euler(-angle, 0, 0));
            }
            else if (currentChar == 'y') // Rotate ccw around Y axis
            {
                workingBranchState.transform *= Matrix4x4.Rotate(Quaternion.Euler(0, +angle, 0));
            }
            else if (currentChar == 'Y') // Rotate cw around Y axis
            {
                workingBranchState.transform *= Matrix4x4.Rotate(Quaternion.Euler(0, -angle, 0));
            }
            else if (currentChar == 'z') // Rotate ccw around Z axis
            {
                workingBranchState.transform *= Matrix4x4.Rotate(Quaternion.Euler(0, 0, +angle));
            }
            else if (currentChar == 'Z') // Rotate cw around Z axis
            {
                workingBranchState.transform *= Matrix4x4.Rotate(Quaternion.Euler(0, 0, -angle));
            }
            else if (currentChar == 'r') // Reduce radius
            {
                workingBranchState.bRadius = workingBranchState.tRadius;
                workingBranchState.tRadius *= radiusDecay;
            }
            else if (currentChar == 'R') // Increase radius
            {
                workingBranchState.bRadius = workingBranchState.tRadius;
                workingBranchState.tRadius *= 1f / radiusDecay;
            }
            else if (currentChar == 's') // Reduce length
            {
                workingBranchState.length *= lengthDecay;
            }
            else if (currentChar == 'S') // Increase length
            {
                workingBranchState.length *= 1 / lengthDecay;
            }
            else if (currentChar == '[') // Save transform
            {
                branchStateStack.Add(workingBranchState);
            }
            else if (currentChar == ']') // Recall transform
            {
                int lastIndex = branchStateStack.Count - 1;
                workingBranchState = branchStateStack[lastIndex];
                branchStateStack.RemoveAt(lastIndex);
            }
        }

        if (mesh != null)
        {
            foreach (Branch branch in branches)
            {
                branchMeshInstances.Add(BranchMeshBuilder.CreateBranch(branch));
            }
            

            Mesh branchesMesh = new Mesh();
            branchesMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            branchesMesh.CombineMeshes(branchMeshInstances.ToArray());

            Mesh leavesMesh = new Mesh();
            leavesMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            leavesMesh.CombineMeshes(leafMeshInstances.ToArray());

            CombineInstance branchesInstance = new CombineInstance();
            branchesInstance.mesh = branchesMesh;

            CombineInstance leavesInstance = new CombineInstance();
            leavesInstance.mesh = leavesMesh;

            CombineInstance[] meshInstances = new CombineInstance[]
            {
                branchesInstance,
                leavesInstance
            };

            mesh.CombineMeshes(meshInstances, false, false);
        }
    }

    [ContextMenu("Update Mesh")]
    void UpdateMesh()
    {
        Mesh treeMesh = new Mesh();
        treeMesh.name = "Generated Tree Mesh";
        treeMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        GetComponent<MeshFilter>().mesh = treeMesh;
        
        GenerateFoliage(treeMesh);
    }

    private void OnValidate()
    {
        UpdateMesh();
    }

    void OnDrawGizmos()
    {
        GenerateFoliage();

        Gizmos.matrix = transform.localToWorldMatrix;

        if (drawBranchLines)
        {
            Gizmos.color = Color.white;
            foreach (Branch branch in branches)
            {
                Vector3 currentPos = branch.transform.GetPosition();
                Matrix4x4 t = branch.transform * Matrix4x4.Translate(Vector3.down * branch.length);

                Gizmos.DrawLine(currentPos, t.GetPosition());
            }
        }
    }
}
