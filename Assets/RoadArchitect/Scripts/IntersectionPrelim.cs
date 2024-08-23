using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RoadArchitect
{
    public class IntersectionPrelim : MonoBehaviour
    {
        #region "Intersection Prelim"
        public static void RoadJobPrelimInter(ref Road _road)
        {
            SplineC spline = _road.spline;
            float roadWidth = _road.RoadWidth();
            float shoulderWidth = _road.shoulderWidth;
            float roadSeperation = roadWidth / 2f;
            float roadSeperationNoTurn = roadWidth / 2f;
            float shoulderSeperation = roadSeperation + shoulderWidth;
            float laneWidth = _road.laneWidth;
            float roadSep1Lane = (roadSeperation + (laneWidth * 0.5f));
            float roadSep2Lane = (roadSeperation + (laneWidth * 1.5f));
            Vector3 POS = default(Vector3);
            bool isPastInter = false;
            bool isOldMethod = false;

            //If left collides with left, etc

            //This will speed up later calculations for intersection 4 corner construction:
            int nodeCount = spline.GetNodeCount();
            float PreInter_RoadWidthMod = 4.5f;
            if (!isOldMethod)
            {
                PreInter_RoadWidthMod = 5.5f;
            }
            float preInterDistance = (spline.RoadWidth * PreInter_RoadWidthMod) / spline.distance;
            SplineN iNode;
            for (int j = 0; j < nodeCount; j++)
            {
                if (!spline.nodes[j].isIntersection)
                {
                    continue;
                }

                iNode = spline.nodes[j];
                //First node set min / max float:
                if (iNode.intersectionConstruction == null)
                {
                    iNode.intersectionConstruction = new iConstructionMaker();
                }
                if (!iNode.intersectionConstruction.isTempConstructionProcessedInter1)
                {
                    preInterDistance = (iNode.spline.RoadWidth * PreInter_RoadWidthMod) / iNode.spline.distance;
                    iNode.intersectionConstruction.tempconstruction_InterStart = iNode.time - preInterDistance;
                    iNode.intersectionConstruction.tempconstruction_InterEnd = iNode.time + preInterDistance;

                    iNode.intersectionConstruction.ClampConstructionValues();

                    iNode.intersectionConstruction.isTempConstructionProcessedInter1 = true;
                }

                if (string.Compare(iNode.uID, iNode.intersection.node1.uID) == 0)
                {
                    iNode = iNode.intersection.node2;
                }
                else
                {
                    iNode = iNode.intersection.node1;
                }

                //Grab other intersection node and set min / max float	
                try
                {
                    if (!iNode.intersectionConstruction.isTempConstructionProcessedInter1)
                    {
                        preInterDistance = (iNode.spline.RoadWidth * PreInter_RoadWidthMod) / iNode.spline.distance;
                        iNode.intersectionConstruction.tempconstruction_InterStart = iNode.time - preInterDistance;
                        iNode.intersectionConstruction.tempconstruction_InterEnd = iNode.time + preInterDistance;

                        iNode.intersectionConstruction.ClampConstructionValues();

                        iNode.intersectionConstruction.isTempConstructionProcessedInter1 = true;
                    }
                }
                catch
                {
                    //Do nothing
                }
            }

            //Now get the four points per intersection:
            SplineN oNode1 = null;
            SplineN oNode2 = null;
            float PreInterPrecision1 = -1f;
            float PreInterPrecision2 = -1f;
            Vector3 PreInterVect = default(Vector3);
            Vector3 PreInterVectR = default(Vector3);
            Vector3 PreInterVectR_RightTurn = default(Vector3);
            Vector3 PreInterVectL = default(Vector3);
            Vector3 PreInterVectL_RightTurn = default(Vector3);
            RoadIntersection roadIntersection = null;


            for (int j = 0; j < nodeCount; j++)
            {
                oNode1 = spline.nodes[j];
                if (oNode1.isIntersection)
                {
                    oNode1 = oNode1.intersection.node1;
                    oNode2 = oNode1.intersection.node2;
                    if (isOldMethod)
                    {
                        PreInterPrecision1 = 0.1f / oNode1.spline.distance;
                        PreInterPrecision2 = 0.1f / oNode2.spline.distance;
                    }
                    else
                    {
                        PreInterPrecision1 = 4f / oNode1.spline.distance;
                        PreInterPrecision2 = 4f / oNode2.spline.distance;
                    }
                    roadIntersection = oNode1.intersection;
                    try
                    {
                        if (oNode1.intersectionConstruction.isTempConstructionProcessedInter2 && oNode2.intersectionConstruction.isTempConstructionProcessedInter2)
                        {
                            continue;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                    roadIntersection = oNode1.intersection;
                    roadIntersection.isCornerRR1Enabled = false;
                    roadIntersection.isCornerRR2Enabled = false;
                    roadIntersection.isCornerRL1Enabled = false;
                    roadIntersection.isCornerRL2Enabled = false;
                    roadIntersection.isCornerLR1Enabled = false;
                    roadIntersection.isCornerLR2Enabled = false;
                    roadIntersection.isCornerLL1Enabled = false;
                    roadIntersection.isCornerLL2Enabled = false;

                    if (!oNode1.intersectionConstruction.isTempConstructionProcessedInter2)
                    {
                        oNode1.intersectionConstruction.tempconstruction_R = new List<Vector2>();
                        oNode1.intersectionConstruction.tempconstruction_L = new List<Vector2>();
                        if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                        {
                            oNode1.intersectionConstruction.tempconstruction_R_RightTurn = new List<Vector2>();
                            oNode1.intersectionConstruction.tempconstruction_L_RightTurn = new List<Vector2>();
                        }

                        for (float i = oNode1.intersectionConstruction.tempconstruction_InterStart; i < oNode1.intersectionConstruction.tempconstruction_InterEnd; i += PreInterPrecision1)
                        {
                            oNode1.spline.GetSplineValueBoth(i, out PreInterVect, out POS);

                            isPastInter = oNode1.spline.IntersectionIsPast(ref i, ref oNode1);
                            if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                            {
                                if (isPastInter)
                                {
                                    PreInterVectR = (PreInterVect + new Vector3(roadSep1Lane * POS.normalized.z, 0, roadSep1Lane * -POS.normalized.x));
                                    PreInterVectL = (PreInterVect + new Vector3(roadSep2Lane * -POS.normalized.z, 0, roadSep2Lane * POS.normalized.x));
                                }
                                else
                                {
                                    PreInterVectR = (PreInterVect + new Vector3(roadSep2Lane * POS.normalized.z, 0, roadSep2Lane * -POS.normalized.x));
                                    PreInterVectL = (PreInterVect + new Vector3(roadSep1Lane * -POS.normalized.z, 0, roadSep1Lane * POS.normalized.x));
                                }
                            }
                            else if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.TurnLane)
                            {
                                PreInterVectR = (PreInterVect + new Vector3(roadSep1Lane * POS.normalized.z, 0, roadSep1Lane * -POS.normalized.x));
                                PreInterVectL = (PreInterVect + new Vector3(roadSep1Lane * -POS.normalized.z, 0, roadSep1Lane * POS.normalized.x));
                            }
                            else
                            {
                                PreInterVectR = (PreInterVect + new Vector3(roadSeperationNoTurn * POS.normalized.z, 0, roadSeperationNoTurn * -POS.normalized.x));
                                PreInterVectL = (PreInterVect + new Vector3(roadSeperationNoTurn * -POS.normalized.z, 0, roadSeperationNoTurn * POS.normalized.x));
                            }

                            oNode1.intersectionConstruction.tempconstruction_R.Add(new Vector2(PreInterVectR.x, PreInterVectR.z));
                            oNode1.intersectionConstruction.tempconstruction_L.Add(new Vector2(PreInterVectL.x, PreInterVectL.z));

                            if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                            {
                                PreInterVectR_RightTurn = (PreInterVect + new Vector3(roadSep2Lane * POS.normalized.z, 0, roadSep2Lane * -POS.normalized.x));
                                oNode1.intersectionConstruction.tempconstruction_R_RightTurn.Add(ConvertVect3ToVect2(PreInterVectR_RightTurn));

                                PreInterVectL_RightTurn = (PreInterVect + new Vector3(roadSep2Lane * -POS.normalized.z, 0, roadSep2Lane * POS.normalized.x));
                                oNode1.intersectionConstruction.tempconstruction_L_RightTurn.Add(ConvertVect3ToVect2(PreInterVectL_RightTurn));
                            }
                        }
                    }

                    //Process second node:
                    if (oNode2.intersectionConstruction == null)
                    {
                        oNode2.intersectionConstruction = new iConstructionMaker();
                    }
                    if (!oNode2.intersectionConstruction.isTempConstructionProcessedInter2)
                    {
                        oNode2.intersectionConstruction.tempconstruction_R = new List<Vector2>();
                        oNode2.intersectionConstruction.tempconstruction_L = new List<Vector2>();
                        if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                        {
                            oNode2.intersectionConstruction.tempconstruction_R_RightTurn = new List<Vector2>();
                            oNode2.intersectionConstruction.tempconstruction_L_RightTurn = new List<Vector2>();
                        }

                        for (float i = oNode2.intersectionConstruction.tempconstruction_InterStart; i < oNode2.intersectionConstruction.tempconstruction_InterEnd; i += PreInterPrecision2)
                        {
                            oNode2.spline.GetSplineValueBoth(i, out PreInterVect, out POS);

                            isPastInter = oNode2.spline.IntersectionIsPast(ref i, ref oNode2);
                            if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                            {
                                if (isPastInter)
                                {
                                    PreInterVectR = (PreInterVect + new Vector3(roadSep1Lane * POS.normalized.z, 0, roadSep1Lane * -POS.normalized.x));
                                    PreInterVectL = (PreInterVect + new Vector3(roadSep2Lane * -POS.normalized.z, 0, roadSep2Lane * POS.normalized.x));
                                }
                                else
                                {
                                    PreInterVectR = (PreInterVect + new Vector3(roadSep2Lane * POS.normalized.z, 0, roadSep2Lane * -POS.normalized.x));
                                    PreInterVectL = (PreInterVect + new Vector3(roadSep1Lane * -POS.normalized.z, 0, roadSep1Lane * POS.normalized.x));
                                }
                            }
                            else if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.TurnLane)
                            {
                                PreInterVectR = (PreInterVect + new Vector3(roadSep1Lane * POS.normalized.z, 0, roadSep1Lane * -POS.normalized.x));
                                PreInterVectL = (PreInterVect + new Vector3(roadSep1Lane * -POS.normalized.z, 0, roadSep1Lane * POS.normalized.x));
                            }
                            else
                            {
                                PreInterVectR = (PreInterVect + new Vector3(roadSeperationNoTurn * POS.normalized.z, 0, roadSeperationNoTurn * -POS.normalized.x));
                                PreInterVectL = (PreInterVect + new Vector3(roadSeperationNoTurn * -POS.normalized.z, 0, roadSeperationNoTurn * POS.normalized.x));
                            }

                            oNode2.intersectionConstruction.tempconstruction_R.Add(new Vector2(PreInterVectR.x, PreInterVectR.z));
                            oNode2.intersectionConstruction.tempconstruction_L.Add(new Vector2(PreInterVectL.x, PreInterVectL.z));
                            if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                            {
                                PreInterVectR_RightTurn = (PreInterVect + new Vector3(roadSep2Lane * POS.normalized.z, 0, roadSep2Lane * -POS.normalized.x));
                                oNode2.intersectionConstruction.tempconstruction_R_RightTurn.Add(ConvertVect3ToVect2(PreInterVectR_RightTurn));

                                PreInterVectL_RightTurn = (PreInterVect + new Vector3(roadSep2Lane * -POS.normalized.z, 0, roadSep2Lane * POS.normalized.x));
                                oNode2.intersectionConstruction.tempconstruction_L_RightTurn.Add(ConvertVect3ToVect2(PreInterVectL_RightTurn));
                            }
                        }
                    }



                    bool isFlipped = false;
                    bool isFlippedSet = false;
                    int hCount1 = oNode1.intersectionConstruction.tempconstruction_R.Count;
                    int hCount2 = oNode2.intersectionConstruction.tempconstruction_R.Count;
                    int N1RCount = oNode1.intersectionConstruction.tempconstruction_R.Count;
                    int N1LCount = oNode1.intersectionConstruction.tempconstruction_L.Count;
                    int N2RCount = oNode2.intersectionConstruction.tempconstruction_R.Count;
                    int N2LCount = oNode2.intersectionConstruction.tempconstruction_L.Count;

                    int[] tCounts = new int[4];
                    tCounts[0] = N1RCount;
                    tCounts[1] = N1LCount;
                    tCounts[2] = N2RCount;
                    tCounts[3] = N2LCount;

                    //RR:
                    int MaxCount = -1;
                    MaxCount = Mathf.Max(N2RCount, N2LCount);
                    for (int h = 0; h < hCount1; h++)
                    {
                        for (int k = 0; k < MaxCount; k++)
                        {
                            if (k < N2RCount)
                            {
                                if (Vector2.Distance(oNode1.intersectionConstruction.tempconstruction_R[h], oNode2.intersectionConstruction.tempconstruction_R[k]) < _road.roadDefinition)
                                {
                                    isFlipped = false;
                                    isFlippedSet = true;
                                    break;
                                }
                            }
                            if (k < N2LCount)
                            {
                                if (Vector2.Distance(oNode1.intersectionConstruction.tempconstruction_R[h], oNode2.intersectionConstruction.tempconstruction_L[k]) < _road.roadDefinition)
                                {
                                    isFlipped = true;
                                    isFlippedSet = true;
                                    break;
                                }
                            }
                        }
                        if (isFlippedSet)
                        {
                            break;
                        }
                    }
                    oNode1.intersection.isFlipped = isFlipped;


                    //Three-way intersections lane specifics:
                    roadIntersection.isNode2BLeftTurnLane = true;
                    roadIntersection.isNode2BRightTurnLane = true;
                    roadIntersection.isNode2FLeftTurnLane = true;
                    roadIntersection.isNode2FRightTurnLane = true;

                    //Three-way intersections:
                    roadIntersection.ignoreSide = -1;
                    roadIntersection.ignoreCorner = -1;
                    roadIntersection.intersectionType = RoadIntersection.IntersectionTypeEnum.FourWay;
                    if (roadIntersection.isFirstSpecialFirst)
                    {
                        roadIntersection.ignoreSide = 3;
                        roadIntersection.intersectionType = RoadIntersection.IntersectionTypeEnum.ThreeWay;
                        if (roadIntersection.intersectionStopType == RoadIntersection.iStopTypeEnum.StopSign_AllWay)
                        {
                            roadIntersection.ignoreCorner = 0;
                        }
                        else if (roadIntersection.intersectionStopType == RoadIntersection.iStopTypeEnum.TrafficLight1 || roadIntersection.intersectionStopType == RoadIntersection.iStopTypeEnum.TrafficLight2)
                        {
                            roadIntersection.ignoreCorner = 1;
                        }

                        if (!oNode1.intersection.isFlipped)
                        {
                            roadIntersection.isNode2FLeftTurnLane = false;
                            roadIntersection.isNode2BRightTurnLane = false;
                        }
                        else
                        {
                            roadIntersection.isNode2BLeftTurnLane = false;
                            roadIntersection.isNode2FRightTurnLane = false;
                        }
                    }
                    else if (roadIntersection.isFirstSpecialLast)
                    {
                        roadIntersection.ignoreSide = 1;
                        roadIntersection.intersectionType = RoadIntersection.IntersectionTypeEnum.ThreeWay;
                        if (roadIntersection.intersectionStopType == RoadIntersection.iStopTypeEnum.StopSign_AllWay)
                        {
                            roadIntersection.ignoreCorner = 2;
                        }
                        else if (roadIntersection.intersectionStopType == RoadIntersection.iStopTypeEnum.TrafficLight1 || roadIntersection.intersectionStopType == RoadIntersection.iStopTypeEnum.TrafficLight2)
                        {
                            roadIntersection.ignoreCorner = 3;
                        }

                        if (!oNode1.intersection.isFlipped)
                        {
                            roadIntersection.isNode2BLeftTurnLane = false;
                            roadIntersection.isNode2FRightTurnLane = false;
                        }
                        else
                        {
                            roadIntersection.isNode2FLeftTurnLane = false;
                            roadIntersection.isNode2BRightTurnLane = false;
                        }

                    }
                    if (!isFlipped)
                    {
                        if (roadIntersection.isSecondSpecialFirst)
                        {
                            roadIntersection.ignoreSide = 2;
                            roadIntersection.intersectionType = RoadIntersection.IntersectionTypeEnum.ThreeWay;
                            if (roadIntersection.intersectionStopType == RoadIntersection.iStopTypeEnum.StopSign_AllWay)
                            {
                                roadIntersection.ignoreCorner = 3;
                            }
                            else if (roadIntersection.intersectionStopType == RoadIntersection.iStopTypeEnum.TrafficLight1 || roadIntersection.intersectionStopType == RoadIntersection.iStopTypeEnum.TrafficLight2)
                            {
                                roadIntersection.ignoreCorner = 0;
                            }

                            if (!oNode1.intersection.isFlipped)
                            {
                                roadIntersection.isNode2BLeftTurnLane = false;
                                roadIntersection.isNode2FRightTurnLane = false;
                            }
                            else
                            {
                                roadIntersection.isNode2FLeftTurnLane = false;
                                roadIntersection.isNode2BRightTurnLane = false;
                            }

                        }
                        else if (roadIntersection.isSecondSpecialLast)
                        {
                            roadIntersection.ignoreSide = 0;
                            roadIntersection.intersectionType = RoadIntersection.IntersectionTypeEnum.ThreeWay;
                            if (roadIntersection.intersectionStopType == RoadIntersection.iStopTypeEnum.StopSign_AllWay)
                            {
                                roadIntersection.ignoreCorner = 1;
                            }
                            else if (roadIntersection.intersectionStopType == RoadIntersection.iStopTypeEnum.TrafficLight1 || roadIntersection.intersectionStopType == RoadIntersection.iStopTypeEnum.TrafficLight2)
                            {
                                roadIntersection.ignoreCorner = 2;
                            }

                            if (!oNode1.intersection.isFlipped)
                            {
                                roadIntersection.isNode2BLeftTurnLane = false;
                                roadIntersection.isNode2FRightTurnLane = false;
                            }
                            else
                            {
                                roadIntersection.isNode2FLeftTurnLane = false;
                                roadIntersection.isNode2BRightTurnLane = false;
                            }

                        }
                    }
                    else
                    {
                        if (roadIntersection.isSecondSpecialFirst)
                        {
                            roadIntersection.ignoreSide = 0;
                            roadIntersection.intersectionType = RoadIntersection.IntersectionTypeEnum.ThreeWay;
                            if (roadIntersection.intersectionStopType == RoadIntersection.iStopTypeEnum.StopSign_AllWay)
                            {
                                roadIntersection.ignoreCorner = 1;
                            }
                            else if (roadIntersection.intersectionStopType == RoadIntersection.iStopTypeEnum.TrafficLight1 || roadIntersection.intersectionStopType == RoadIntersection.iStopTypeEnum.TrafficLight2)
                            {
                                roadIntersection.ignoreCorner = 2;
                            }

                            if (!oNode1.intersection.isFlipped)
                            {
                                roadIntersection.isNode2BLeftTurnLane = false;
                                roadIntersection.isNode2FRightTurnLane = false;
                            }
                            else
                            {
                                roadIntersection.isNode2FLeftTurnLane = false;
                                roadIntersection.isNode2BRightTurnLane = false;
                            }

                        }
                        else if (roadIntersection.isSecondSpecialLast)
                        {
                            roadIntersection.ignoreSide = 2;
                            roadIntersection.intersectionType = RoadIntersection.IntersectionTypeEnum.ThreeWay;
                            if (roadIntersection.intersectionStopType == RoadIntersection.iStopTypeEnum.StopSign_AllWay)
                            {
                                roadIntersection.ignoreCorner = 3;
                            }
                            else if (roadIntersection.intersectionStopType == RoadIntersection.iStopTypeEnum.TrafficLight1 || roadIntersection.intersectionStopType == RoadIntersection.iStopTypeEnum.TrafficLight2)
                            {
                                roadIntersection.ignoreCorner = 0;
                            }

                            if (!oNode1.intersection.isFlipped)
                            {
                                roadIntersection.isNode2BLeftTurnLane = false;
                                roadIntersection.isNode2FRightTurnLane = false;
                            }
                            else
                            {
                                roadIntersection.isNode2FLeftTurnLane = false;
                                roadIntersection.isNode2BRightTurnLane = false;
                            }
                        }
                    }

                    //Find corners:
                    Vector2 tFoundVectRR = default(Vector2);
                    Vector2 tFoundVectRL = default(Vector2);
                    Vector2 tFoundVectLR = default(Vector2);
                    Vector2 tFoundVectLL = default(Vector2);
                    if (!isOldMethod)
                    {
                        //RR:
                        if (!isFlipped)
                        {
                            if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                            {
                                tFoundVectRR = IntersectionCornerCalc(ref oNode1.intersectionConstruction.tempconstruction_R_RightTurn, ref oNode2.intersectionConstruction.tempconstruction_R);
                            }
                            else
                            {
                                tFoundVectRR = IntersectionCornerCalc(ref oNode1.intersectionConstruction.tempconstruction_R, ref oNode2.intersectionConstruction.tempconstruction_R);
                            }
                        }
                        else
                        {
                            if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                            {
                                tFoundVectRR = IntersectionCornerCalc(ref oNode1.intersectionConstruction.tempconstruction_R_RightTurn, ref oNode2.intersectionConstruction.tempconstruction_L);
                            }
                            else
                            {
                                tFoundVectRR = IntersectionCornerCalc(ref oNode1.intersectionConstruction.tempconstruction_R, ref oNode2.intersectionConstruction.tempconstruction_L);
                            }
                        }

                        //RL:
                        if (!isFlipped)
                        {
                            if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                            {
                                tFoundVectRL = IntersectionCornerCalc(ref oNode1.intersectionConstruction.tempconstruction_R, ref oNode2.intersectionConstruction.tempconstruction_L_RightTurn);
                            }
                            else
                            {
                                tFoundVectRL = IntersectionCornerCalc(ref oNode1.intersectionConstruction.tempconstruction_R, ref oNode2.intersectionConstruction.tempconstruction_L);
                            }
                        }
                        else
                        {
                            if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                            {
                                tFoundVectRL = IntersectionCornerCalc(ref oNode1.intersectionConstruction.tempconstruction_R, ref oNode2.intersectionConstruction.tempconstruction_R_RightTurn);
                            }
                            else
                            {
                                tFoundVectRL = IntersectionCornerCalc(ref oNode1.intersectionConstruction.tempconstruction_R, ref oNode2.intersectionConstruction.tempconstruction_R);
                            }
                        }

                        //LL:
                        if (!isFlipped)
                        {
                            if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                            {
                                tFoundVectLL = IntersectionCornerCalc(ref oNode1.intersectionConstruction.tempconstruction_L_RightTurn, ref oNode2.intersectionConstruction.tempconstruction_L);
                            }
                            else
                            {
                                tFoundVectLL = IntersectionCornerCalc(ref oNode1.intersectionConstruction.tempconstruction_L, ref oNode2.intersectionConstruction.tempconstruction_L);
                            }
                        }
                        else
                        {
                            if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                            {
                                tFoundVectLL = IntersectionCornerCalc(ref oNode1.intersectionConstruction.tempconstruction_L_RightTurn, ref oNode2.intersectionConstruction.tempconstruction_R);
                            }
                            else
                            {
                                tFoundVectLL = IntersectionCornerCalc(ref oNode1.intersectionConstruction.tempconstruction_L, ref oNode2.intersectionConstruction.tempconstruction_R);
                            }
                        }

                        //LR:
                        if (!isFlipped)
                        {
                            if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                            {
                                tFoundVectLR = IntersectionCornerCalc(ref oNode1.intersectionConstruction.tempconstruction_L, ref oNode2.intersectionConstruction.tempconstruction_R_RightTurn);
                            }
                            else
                            {
                                tFoundVectLR = IntersectionCornerCalc(ref oNode1.intersectionConstruction.tempconstruction_L, ref oNode2.intersectionConstruction.tempconstruction_R);
                            }
                        }
                        else
                        {
                            if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                            {
                                tFoundVectLR = IntersectionCornerCalc(ref oNode1.intersectionConstruction.tempconstruction_L, ref oNode2.intersectionConstruction.tempconstruction_L_RightTurn);
                            }
                            else
                            {
                                tFoundVectLR = IntersectionCornerCalc(ref oNode1.intersectionConstruction.tempconstruction_L, ref oNode2.intersectionConstruction.tempconstruction_L);
                            }
                        }
                    }
                    else
                    {
                        //Now two lists of R and L on each intersection node, now match:
                        float eDistanceRR = 5000f;
                        float oDistanceRR = 0f;
                        float eDistanceRL = 5000f;
                        float oDistanceRL = 0f;
                        float eDistanceLR = 5000f;
                        float oDistanceLR = 0f;
                        float eDistanceLL = 5000f;
                        float oDistanceLL = 0f;
                        bool isHasBeen1mRR = false;
                        bool isHasBeen1mRL = false;
                        bool isHasBeen1mLR = false;
                        bool isHasBeen1mLL = false;
                        bool isHasBeen1mRR_ignore = false;
                        bool isHasBeen1mRL_ignore = false;
                        bool isHasBeen1mLR_ignore = false;
                        bool isHasBeen1mLL_ignore = false;
                        bool isHasBeen1mRRIgnoreMax = false;
                        bool isHasBeen1mRLIgnoreMax = false;
                        bool isHasBeen1mLRIgnoreMax = false;
                        bool isHasBeen1mLLIgnoreMax = false;
                        float mMin = 0.2f;
                        float mMax = 0.5f;

                        MaxCount = Mathf.Max(tCounts);
                        int MaxHCount = Mathf.Max(hCount1, hCount2);
                        for (int h = 0; h < MaxHCount; h++)
                        {
                            isHasBeen1mRR = false;
                            isHasBeen1mRL = false;
                            isHasBeen1mLR = false;
                            isHasBeen1mLL = false;
                            isHasBeen1mRR_ignore = false;
                            isHasBeen1mRL_ignore = false;
                            isHasBeen1mLR_ignore = false;
                            isHasBeen1mLL_ignore = false;
                            for (int k = 0; k < MaxCount; k++)
                            {
                                if (!isFlipped)
                                {
                                    //RR:
                                    if (!isHasBeen1mRRIgnoreMax && !isHasBeen1mRR_ignore && (h < N1RCount && k < N2RCount))
                                    {
                                        oDistanceRR = Vector2.Distance(oNode1.intersectionConstruction.tempconstruction_R[h], oNode2.intersectionConstruction.tempconstruction_R[k]);
                                        if (oDistanceRR < eDistanceRR)
                                        {
                                            eDistanceRR = oDistanceRR;
                                            tFoundVectRR = oNode1.intersectionConstruction.tempconstruction_R[h]; //RR
                                            if (eDistanceRR < 0.07f)
                                            {
                                                isHasBeen1mRRIgnoreMax = true;
                                            }
                                        }
                                        if (oDistanceRR > mMax && isHasBeen1mRR)
                                        {
                                            isHasBeen1mRR_ignore = true;
                                        }
                                        if (oDistanceRR < mMin)
                                        {
                                            isHasBeen1mRR = true;
                                        }
                                    }
                                    //RL:
                                    if (!isHasBeen1mRLIgnoreMax && !isHasBeen1mRL_ignore && (h < N1RCount && k < N2LCount))
                                    {
                                        oDistanceRL = Vector2.Distance(oNode1.intersectionConstruction.tempconstruction_R[h], oNode2.intersectionConstruction.tempconstruction_L[k]);
                                        if (oDistanceRL < eDistanceRL)
                                        {
                                            eDistanceRL = oDistanceRL;
                                            tFoundVectRL = oNode1.intersectionConstruction.tempconstruction_R[h]; //RL
                                            if (eDistanceRL < 0.07f)
                                            {
                                                isHasBeen1mRLIgnoreMax = true;
                                            }
                                        }
                                        if (oDistanceRL > mMax && isHasBeen1mRL)
                                        {
                                            isHasBeen1mRL_ignore = true;
                                        }
                                        if (oDistanceRL < mMin)
                                        {
                                            isHasBeen1mRL = true;
                                        }
                                    }
                                    //LR:
                                    if (!isHasBeen1mLRIgnoreMax && !isHasBeen1mLR_ignore && (h < N1LCount && k < N2RCount))
                                    {
                                        oDistanceLR = Vector2.Distance(oNode1.intersectionConstruction.tempconstruction_L[h], oNode2.intersectionConstruction.tempconstruction_R[k]);
                                        if (oDistanceLR < eDistanceLR)
                                        {
                                            eDistanceLR = oDistanceLR;
                                            tFoundVectLR = oNode1.intersectionConstruction.tempconstruction_L[h]; //LR
                                            if (eDistanceLR < 0.07f)
                                            {
                                                isHasBeen1mLRIgnoreMax = true;
                                            }
                                        }
                                        if (oDistanceLR > mMax && isHasBeen1mLR)
                                        {
                                            isHasBeen1mLR_ignore = true;
                                        }
                                        if (oDistanceLR < mMin)
                                        {
                                            isHasBeen1mLR = true;
                                        }
                                    }
                                    //LL:
                                    if (!isHasBeen1mLLIgnoreMax && !isHasBeen1mLL_ignore && (h < N1LCount && k < N2LCount))
                                    {
                                        oDistanceLL = Vector2.Distance(oNode1.intersectionConstruction.tempconstruction_L[h], oNode2.intersectionConstruction.tempconstruction_L[k]);
                                        if (oDistanceLL < eDistanceLL)
                                        {
                                            eDistanceLL = oDistanceLL;
                                            tFoundVectLL = oNode1.intersectionConstruction.tempconstruction_L[h]; //LL
                                            if (eDistanceLL < 0.07f)
                                            {
                                                isHasBeen1mLLIgnoreMax = true;
                                            }
                                        }
                                        if (oDistanceLL > mMax && isHasBeen1mLL)
                                        {
                                            isHasBeen1mLL_ignore = true;
                                        }
                                        if (oDistanceLL < mMin)
                                        {
                                            isHasBeen1mLL = true;
                                        }
                                    }
                                }
                                else
                                {
                                    //RR:
                                    if (!isHasBeen1mRRIgnoreMax && !isHasBeen1mRR_ignore && (h < N1RCount && k < N2LCount))
                                    {
                                        oDistanceRR = Vector2.Distance(oNode1.intersectionConstruction.tempconstruction_R[h], oNode2.intersectionConstruction.tempconstruction_L[k]);
                                        if (oDistanceRR < eDistanceRR)
                                        {
                                            eDistanceRR = oDistanceRR;
                                            tFoundVectRR = oNode1.intersectionConstruction.tempconstruction_R[h]; //RR
                                            if (eDistanceRR < 0.07f)
                                            {
                                                isHasBeen1mRRIgnoreMax = true;
                                            }
                                        }
                                        if (oDistanceRR > mMax && isHasBeen1mRR)
                                        {
                                            isHasBeen1mRR_ignore = true;
                                        }
                                        if (oDistanceRR < mMin)
                                        {
                                            isHasBeen1mRR = true;
                                        }
                                    }
                                    //RL:
                                    if (!isHasBeen1mRLIgnoreMax && !isHasBeen1mRL_ignore && (h < N1RCount && k < N2RCount))
                                    {
                                        oDistanceRL = Vector2.Distance(oNode1.intersectionConstruction.tempconstruction_R[h], oNode2.intersectionConstruction.tempconstruction_R[k]);
                                        if (oDistanceRL < eDistanceRL)
                                        {
                                            eDistanceRL = oDistanceRL;
                                            tFoundVectRL = oNode1.intersectionConstruction.tempconstruction_R[h]; //RL
                                            if (eDistanceRL < 0.07f)
                                            {
                                                isHasBeen1mRLIgnoreMax = true;
                                            }
                                        }
                                        if (oDistanceRL > mMax && isHasBeen1mRL)
                                        {
                                            isHasBeen1mRL_ignore = true;
                                        }
                                        if (oDistanceRL < mMin)
                                        {
                                            isHasBeen1mRL = true;
                                        }
                                    }
                                    //LR:
                                    if (!isHasBeen1mLRIgnoreMax && !isHasBeen1mLR_ignore && (h < N1LCount && k < N2LCount))
                                    {
                                        oDistanceLR = Vector2.Distance(oNode1.intersectionConstruction.tempconstruction_L[h], oNode2.intersectionConstruction.tempconstruction_L[k]);
                                        if (oDistanceLR < eDistanceLR)
                                        {
                                            eDistanceLR = oDistanceLR;
                                            tFoundVectLR = oNode1.intersectionConstruction.tempconstruction_L[h]; //LR
                                            if (eDistanceLR < 0.07f)
                                            {
                                                isHasBeen1mLRIgnoreMax = true;
                                            }
                                        }
                                        if (oDistanceLR > mMax && isHasBeen1mLR)
                                        {
                                            isHasBeen1mLR_ignore = true;
                                        }
                                        if (oDistanceLR < mMin)
                                        {
                                            isHasBeen1mLR = true;
                                        }
                                    }
                                    //LL:
                                    if (!isHasBeen1mLLIgnoreMax && !isHasBeen1mLL_ignore && (h < N1LCount && k < N2RCount))
                                    {
                                        oDistanceLL = Vector2.Distance(oNode1.intersectionConstruction.tempconstruction_L[h], oNode2.intersectionConstruction.tempconstruction_R[k]);
                                        if (oDistanceLL < eDistanceLL)
                                        {
                                            eDistanceLL = oDistanceLL;
                                            tFoundVectLL = oNode1.intersectionConstruction.tempconstruction_L[h]; //LL
                                            if (eDistanceLL < 0.07f)
                                            {
                                                isHasBeen1mLLIgnoreMax = true;
                                            }
                                        }
                                        if (oDistanceLL > mMax && isHasBeen1mLL)
                                        {
                                            isHasBeen1mLL_ignore = true;
                                        }
                                        if (oDistanceLL < mMin)
                                        {
                                            isHasBeen1mLL = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    oNode1.intersectionConstruction.isTempConstructionProcessedInter2 = true;
                    oNode2.intersectionConstruction.isTempConstructionProcessedInter2 = true;

                    Vector3 tVectRR = new Vector3(tFoundVectRR.x, 0f, tFoundVectRR.y);
                    Vector3 tVectRL = new Vector3(tFoundVectRL.x, 0f, tFoundVectRL.y);
                    Vector3 tVectLR = new Vector3(tFoundVectLR.x, 0f, tFoundVectLR.y);
                    Vector3 tVectLL = new Vector3(tFoundVectLL.x, 0f, tFoundVectLL.y);

                    oNode1.intersection.cornerRR = tVectRR;
                    oNode1.intersection.cornerRL = tVectRL;
                    oNode1.intersection.cornerLR = tVectLR;
                    oNode1.intersection.cornerLL = tVectLL;

                    float[] tMaxFloats = new float[4];
                    tMaxFloats[0] = Vector3.Distance(((tVectRR - tVectRL) * 0.5f) + tVectRL, oNode1.pos) * 1.25f;
                    tMaxFloats[1] = Vector3.Distance(((tVectRR - tVectLR) * 0.5f) + tVectLR, oNode1.pos) * 1.25f;
                    tMaxFloats[2] = Vector3.Distance(((tVectRL - tVectLL) * 0.5f) + tVectLL, oNode1.pos) * 1.25f;
                    tMaxFloats[3] = Vector3.Distance(((tVectLR - tVectLL) * 0.5f) + tVectLL, oNode1.pos) * 1.25f;
                    roadIntersection.maxInterDistance = Mathf.Max(tMaxFloats);

                    float[] tMaxFloatsSQ = new float[4];
                    tMaxFloatsSQ[0] = Vector3.SqrMagnitude((((tVectRR - tVectRL) * 0.5f) + tVectRL) - oNode1.pos) * 1.25f;
                    tMaxFloatsSQ[1] = Vector3.SqrMagnitude((((tVectRR - tVectLR) * 0.5f) + tVectLR) - oNode1.pos) * 1.25f;
                    tMaxFloatsSQ[2] = Vector3.SqrMagnitude((((tVectRL - tVectLL) * 0.5f) + tVectLL) - oNode1.pos) * 1.25f;
                    tMaxFloatsSQ[3] = Vector3.SqrMagnitude((((tVectLR - tVectLL) * 0.5f) + tVectLL) - oNode1.pos) * 1.25f;
                    roadIntersection.maxInterDistanceSQ = Mathf.Max(tMaxFloatsSQ);

                    float TotalLanes = (int)(roadWidth / laneWidth);
                    float TotalLanesI = TotalLanes;
                    float LanesPerSide = TotalLanes / 2f;

                    if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                    {
                        TotalLanesI = TotalLanes + 2f;
                        //Lower left to lower right: 
                        roadIntersection.cornerLRCornerRR = new Vector3[5];
                        roadIntersection.cornerLRCornerRR[0] = tVectLR;
                        roadIntersection.cornerLRCornerRR[1] = ((tVectRR - tVectLR) * (LanesPerSide / TotalLanesI)) + tVectLR;
                        roadIntersection.cornerLRCornerRR[2] = ((tVectRR - tVectLR) * ((LanesPerSide + 1) / TotalLanesI)) + tVectLR;
                        roadIntersection.cornerLRCornerRR[3] = ((tVectRR - tVectLR) * ((LanesPerSide + 1 + LanesPerSide) / TotalLanesI)) + tVectLR;
                        roadIntersection.cornerLRCornerRR[4] = tVectRR;
                        //Upper right to lower right:
                        roadIntersection.cornerRLCornerRR = new Vector3[5];
                        roadIntersection.cornerRLCornerRR[0] = tVectRL;
                        roadIntersection.cornerRLCornerRR[1] = ((tVectRR - tVectRL) * (1 / TotalLanesI)) + tVectRL;
                        roadIntersection.cornerRLCornerRR[2] = ((tVectRR - tVectRL) * ((LanesPerSide + 1) / TotalLanesI)) + tVectRL;
                        roadIntersection.cornerRLCornerRR[3] = ((tVectRR - tVectRL) * ((LanesPerSide + 2) / TotalLanesI)) + tVectRL;
                        roadIntersection.cornerRLCornerRR[4] = tVectRR;
                        //Upper left to upper right:
                        roadIntersection.cornerLLCornerRL = new Vector3[5];
                        roadIntersection.cornerLLCornerRL[0] = tVectLL;
                        roadIntersection.cornerLLCornerRL[1] = ((tVectRL - tVectLL) * (1 / TotalLanesI)) + tVectLL;
                        roadIntersection.cornerLLCornerRL[2] = ((tVectRL - tVectLL) * ((LanesPerSide + 1) / TotalLanesI)) + tVectLL;
                        roadIntersection.cornerLLCornerRL[3] = ((tVectRL - tVectLL) * ((LanesPerSide + 2) / TotalLanesI)) + tVectLL;
                        roadIntersection.cornerLLCornerRL[4] = tVectRL;
                        //Upper left to lower left:
                        roadIntersection.cornerLLCornerLR = new Vector3[5];
                        roadIntersection.cornerLLCornerLR[0] = tVectLL;
                        roadIntersection.cornerLLCornerLR[1] = ((tVectLR - tVectLL) * (LanesPerSide / TotalLanesI)) + tVectLL;
                        roadIntersection.cornerLLCornerLR[2] = ((tVectLR - tVectLL) * ((LanesPerSide + 1) / TotalLanesI)) + tVectLL;
                        roadIntersection.cornerLLCornerLR[3] = ((tVectLR - tVectLL) * ((LanesPerSide + 1 + LanesPerSide) / TotalLanesI)) + tVectLL;
                        roadIntersection.cornerLLCornerLR[4] = tVectLR;
                    }
                    else if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.TurnLane)
                    {
                        TotalLanesI = TotalLanes + 1;
                        //Lower left to lower right:
                        roadIntersection.cornerLRCornerRR = new Vector3[4];
                        roadIntersection.cornerLRCornerRR[0] = tVectLR;
                        roadIntersection.cornerLRCornerRR[1] = ((tVectRR - tVectLR) * (LanesPerSide / TotalLanesI)) + tVectLR;
                        roadIntersection.cornerLRCornerRR[2] = ((tVectRR - tVectLR) * ((LanesPerSide + 1) / TotalLanesI)) + tVectLR;
                        roadIntersection.cornerLRCornerRR[3] = tVectRR;
                        //Upper right to lower right:
                        roadIntersection.cornerRLCornerRR = new Vector3[4];
                        roadIntersection.cornerRLCornerRR[0] = tVectRL;
                        roadIntersection.cornerRLCornerRR[1] = ((tVectRR - tVectRL) * (LanesPerSide / TotalLanesI)) + tVectRL;
                        roadIntersection.cornerRLCornerRR[2] = ((tVectRR - tVectRL) * ((LanesPerSide + 1) / TotalLanesI)) + tVectRL;
                        roadIntersection.cornerRLCornerRR[3] = tVectRR;
                        //Upper left to upper right:
                        roadIntersection.cornerLLCornerRL = new Vector3[4];
                        roadIntersection.cornerLLCornerRL[0] = tVectLL;
                        roadIntersection.cornerLLCornerRL[1] = ((tVectRL - tVectLL) * (LanesPerSide / TotalLanesI)) + tVectLL;
                        roadIntersection.cornerLLCornerRL[2] = ((tVectRL - tVectLL) * ((LanesPerSide + 1) / TotalLanesI)) + tVectLL;
                        roadIntersection.cornerLLCornerRL[3] = tVectRL;
                        //Upper left to lower left:
                        roadIntersection.cornerLLCornerLR = new Vector3[4];
                        roadIntersection.cornerLLCornerLR[0] = tVectLL;
                        roadIntersection.cornerLLCornerLR[1] = ((tVectLR - tVectLL) * (LanesPerSide / TotalLanesI)) + tVectLL;
                        roadIntersection.cornerLLCornerLR[2] = ((tVectLR - tVectLL) * ((LanesPerSide + 1) / TotalLanesI)) + tVectLL;
                        roadIntersection.cornerLLCornerLR[3] = tVectLR;
                    }
                    else if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.NoTurnLane)
                    {
                        TotalLanesI = TotalLanes + 0;
                        //Lower left to lower right:
                        roadIntersection.cornerLRCornerRR = new Vector3[3];
                        roadIntersection.cornerLRCornerRR[0] = tVectLR;
                        roadIntersection.cornerLRCornerRR[1] = ((tVectRR - tVectLR) * 0.5f) + tVectLR;
                        roadIntersection.cornerLRCornerRR[2] = tVectRR;
                        //Upper right to lower right:
                        roadIntersection.cornerRLCornerRR = new Vector3[3];
                        roadIntersection.cornerRLCornerRR[0] = tVectRL;
                        roadIntersection.cornerRLCornerRR[1] = ((tVectRR - tVectRL) * 0.5f) + tVectRL;
                        roadIntersection.cornerRLCornerRR[2] = tVectRR;
                        //Upper left to upper right:
                        roadIntersection.cornerLLCornerRL = new Vector3[3];
                        roadIntersection.cornerLLCornerRL[0] = tVectLL;
                        roadIntersection.cornerLLCornerRL[1] = ((tVectRL - tVectLL) * 0.5f) + tVectLL;
                        roadIntersection.cornerLLCornerRL[2] = tVectRL;
                        //Upper left to lower left:
                        roadIntersection.cornerLLCornerLR = new Vector3[3];
                        roadIntersection.cornerLLCornerLR[0] = tVectLL;
                        roadIntersection.cornerLLCornerLR[1] = ((tVectLR - tVectLL) * 0.5f) + tVectLL;
                        roadIntersection.cornerLLCornerLR[2] = tVectLR;
                    }

                    //Use node1/node2 for angles instead
                    float tShoulderWidth = shoulderWidth * 1.75f;
                    float tRampWidth = shoulderWidth * 2f;

                    oNode1.intersection.oddAngle = Vector3.Angle(roadIntersection.node2.tangent, roadIntersection.node1.tangent);
                    oNode1.intersection.evenAngle = 180f - Vector3.Angle(roadIntersection.node2.tangent, roadIntersection.node1.tangent);

                    IntersectionObjects.GetFourPoints(roadIntersection, out roadIntersection.cornerRROuter, out roadIntersection.cornerRLOuter, out roadIntersection.cornerLLOuter, out roadIntersection.cornerLROuter, tShoulderWidth);
                    IntersectionObjects.GetFourPoints(roadIntersection, out roadIntersection.cornerRRRampOuter, out roadIntersection.cornerRLRampOuter, out roadIntersection.cornerLLRampOuter, out roadIntersection.cornerLRRampOuter, tRampWidth);

                    roadIntersection.ConstructBoundsRect();
                    roadIntersection.cornerRR2D = new Vector2(tVectRR.x, tVectRR.z);
                    roadIntersection.cornerRL2D = new Vector2(tVectRL.x, tVectRL.z);
                    roadIntersection.cornerLL2D = new Vector2(tVectLL.x, tVectLL.z);
                    roadIntersection.cornerLR2D = new Vector2(tVectLR.x, tVectLR.z);

                    if (!oNode1.intersection.isSameSpline)
                    {
                        if (string.Compare(_road.spline.uID, oNode1.spline.road.spline.uID) != 0)
                        {
                            AddIntersectionBounds(ref oNode1.spline.road, ref _road.RCS);
                        }
                        else if (string.Compare(_road.spline.uID, oNode2.spline.road.spline.uID) != 0)
                        {
                            AddIntersectionBounds(ref oNode2.spline.road, ref _road.RCS);
                        }
                    }
                }
            }
        }


        private static Vector2 IntersectionCornerCalc(ref List<Vector2> _primaryList, ref List<Vector2> _secondaryList)
        {
            int PrimaryCount = _primaryList.Count;
            int SecondaryCount = _secondaryList.Count;
            Vector2 t2D_Line1Start = default(Vector2);
            Vector2 t2D_Line1End = default(Vector2);
            Vector2 t2D_Line2Start = default(Vector2);
            Vector2 t2D_Line2End = default(Vector2);
            bool isDidIntersect = false;
            Vector2 tIntersectLocation = default(Vector2);
            for (int i = 1; i < PrimaryCount; i++)
            {
                isDidIntersect = false;
                t2D_Line1Start = _primaryList[i - 1];
                t2D_Line1End = _primaryList[i];
                for (int k = 1; k < SecondaryCount; k++)
                {
                    isDidIntersect = false;
                    t2D_Line2Start = _secondaryList[k - 1];
                    t2D_Line2End = _secondaryList[k];
                    isDidIntersect = RootUtils.Intersects2D(ref t2D_Line1Start, ref t2D_Line1End, ref t2D_Line2Start, ref t2D_Line2End, out tIntersectLocation);
                    if (isDidIntersect)
                    {
                        return tIntersectLocation;
                    }
                }
            }
            return tIntersectLocation;
        }


        private static void AddIntersectionBounds(ref Road _road, ref RoadConstructorBufferMaker _RCS)
        {
            #region "Vars"
            bool isBridge = false;
            bool isTempBridge = false;

            bool isTunnel = false;
            bool isTempTunnel = false;

            RoadIntersection roadIntersection = null;
            bool isPastInter = false;
            bool isMaxIntersection = false;
            bool isWasPrevMaxInter = false;
            Vector3 tVect = default(Vector3);
            Vector3 POS = default(Vector3);
            float tIntHeight = 0f;
            float tIntStrength = 0f;
            float tIntStrength_temp = 0f;
            //float tIntDistCheck = 75f;
            bool isFirstInterNode = false;
            Vector3 tVect_Prev = default(Vector3);
            Vector3 rVect_Prev = default(Vector3);
            Vector3 lVect_Prev = default(Vector3);
            Vector3 rVect = default(Vector3);
            Vector3 lVect = default(Vector3);
            Vector3 ShoulderR_rVect = default(Vector3);
            Vector3 ShoulderR_lVect = default(Vector3);
            Vector3 ShoulderL_rVect = default(Vector3);
            Vector3 ShoulderL_lVect = default(Vector3);

            Vector3 RampR_R = default(Vector3);
            Vector3 RampR_L = default(Vector3);
            Vector3 RampL_R = default(Vector3);
            Vector3 RampL_L = default(Vector3);

            Vector3 ShoulderR_PrevLVect = default(Vector3);
            Vector3 ShoulderL_PrevRVect = default(Vector3);
            Vector3 ShoulderR_PrevRVect = default(Vector3);
            Vector3 ShoulderL_PrevLVect = default(Vector3);
            //Vector3 ShoulderR_PrevRVect2 = default(Vector3);
            //Vector3 ShoulderL_PrevLVect2 = default(Vector3);
            //Vector3 ShoulderR_PrevRVect3 = default(Vector3);
            //Vector3 ShoulderL_PrevLVect3 = default(Vector3);
            Vector3 RampR_PrevR = default(Vector3);
            Vector3 RampR_PrevL = default(Vector3);
            Vector3 RampL_PrevR = default(Vector3);
            Vector3 RampL_PrevL = default(Vector3);
            SplineC tSpline = _road.spline;
            //Road width:
            float RoadWidth = _road.RoadWidth();
            float ShoulderWidth = _road.shoulderWidth;
            float RoadSeperation = RoadWidth / 2f;
            float RoadSeperation_NoTurn = RoadWidth / 2f;
            float ShoulderSeperation = RoadSeperation + ShoulderWidth;
            float LaneWidth = _road.laneWidth;
            float RoadSep1Lane = (RoadSeperation + (LaneWidth * 0.5f));
            float RoadSep2Lane = (RoadSeperation + (LaneWidth * 1.5f));
            float ShoulderSep1Lane = (ShoulderSeperation + (LaneWidth * 0.5f));
            float ShoulderSep2Lane = (ShoulderSeperation + (LaneWidth * 1.5f));

            float Step = _road.roadDefinition / tSpline.distance;

            SplineN xNode = null;
            float tInterSubtract = 4f;
            float tLastInterHeight = -4f;
            #endregion

            //GameObject xObj = null;
            //xObj = GameObject.Find("temp22");
            //while(xObj != null)
            //{
            //	Object.DestroyImmediate(xObj);
            //	xObj = GameObject.Find("temp22");
            //}
            //xObj = GameObject.Find("temp23");
            //while(xObj != null)
            //{
            //	Object.DestroyImmediate(xObj);
            //	xObj = GameObject.Find("temp23");
            //}
            //xObj = GameObject.Find("temp22_RR");
            //while(xObj != null)
            //{
            //	Object.DestroyImmediate(xObj);
            //	xObj = GameObject.Find("temp22_RR");
            //}
            //xObj = GameObject.Find("temp22_RL");
            //while(xObj != null)
            //{
            //	Object.DestroyImmediate(xObj);
            //	xObj = GameObject.Find("temp22_RL");
            //}
            //xObj = GameObject.Find("temp22_LR");
            //while(xObj != null)
            //{
            //	Object.DestroyImmediate(xObj);
            //	xObj = GameObject.Find("temp22_LR");
            //}
            //xObj = GameObject.Find("temp22_LL");
            //while(xObj != null)
            //{
            //	Object.DestroyImmediate(xObj);
            //	xObj = GameObject.Find("temp22_LL");
            //}

            bool isFinalEnd = false;
            float i = 0f;

            float FinalMax = 1f;
            float StartMin = 0f;
            if (tSpline.isSpecialEndControlNode)
            {
                FinalMax = tSpline.nodes[tSpline.GetNodeCount() - 2].time;
            }
            if (tSpline.isSpecialStartControlNode)
            {
                StartMin = tSpline.nodes[1].time;
            }

            //int StartIndex = tSpline.GetClosestRoadDefIndex(StartMin,true,false);
            //int EndIndex = tSpline.GetClosestRoadDefIndex(FinalMax,false,true);
            bool isSkip = true;
            bool isSkipFinal = false;
            int kCount = 0;
            int kFinalCount = tSpline.RoadDefKeysArray.Length;
            int spamcheckmax1 = 18000;
            int spamcheck1 = 0;

            if (RootUtils.IsApproximately(StartMin, 0f, 0.0001f))
            {
                isSkip = false;
            }
            if (RootUtils.IsApproximately(FinalMax, 1f, 0.0001f))
            {
                isSkipFinal = true;
            }

            while (!isFinalEnd && spamcheck1 < spamcheckmax1)
            {
                spamcheck1++;

                if (isSkip)
                {
                    i = StartMin;
                    isSkip = false;
                }
                else
                {
                    if (kCount >= kFinalCount)
                    {
                        i = FinalMax;
                        if (isSkipFinal)
                        {
                            break;
                        }
                    }
                    else
                    {
                        i = tSpline.TranslateInverseParamToFloat(tSpline.RoadDefKeysArray[kCount]);
                        kCount += 1;
                    }
                }

                if (i > 1f)
                {
                    break;
                }
                if (i < 0f)
                {
                    i = 0f;
                }

                if (RootUtils.IsApproximately(i, FinalMax, 0.00001f))
                {
                    isFinalEnd = true;
                }
                else if (i > FinalMax)
                {
                    if (tSpline.isSpecialEndControlNode)
                    {
                        i = FinalMax;
                        isFinalEnd = true;
                    }
                    else
                    {
                        isFinalEnd = true;
                        break;
                    }
                }

                tSpline.GetSplineValueBoth(i, out tVect, out POS);
                isPastInter = false;
                tIntStrength = tSpline.IntersectionStrength(ref tVect, ref tIntHeight, ref roadIntersection, ref isPastInter, ref i, ref xNode);
                if (RootUtils.IsApproximately(tIntStrength, 1f, 0.001f) || tIntStrength > 1f)
                {
                    isMaxIntersection = true;
                }
                else
                {
                    isMaxIntersection = false;
                }

                if (isMaxIntersection)
                {
                    if (string.Compare(xNode.uID, roadIntersection.node1.uID) == 0)
                    {
                        isFirstInterNode = true;
                    }
                    else
                    {
                        isFirstInterNode = false;
                    }

                    //Convoluted for initial trigger:
                    isTempBridge = tSpline.IsInBridge(i);
                    if (!isBridge && isTempBridge)
                    {
                        isBridge = true;
                    }
                    else if (isBridge && !isTempBridge)
                    {
                        isBridge = false;
                    }
                    //Check if this is the last bridge run for this bridge:
                    if (isBridge)
                    {
                        isTempBridge = tSpline.IsInBridge(i + Step);
                    }


                    //Convoluted for initial trigger:
                    isTempTunnel = tSpline.IsInTunnel(i);
                    if (!isTunnel && isTempTunnel)
                    {
                        isTunnel = true;
                    }
                    else if (isTunnel && !isTempTunnel)
                    {
                        isTunnel = false;
                    }
                    //Check if this is the last Tunnel run for this Tunnel:
                    if (isTunnel)
                    {
                        isTempTunnel = tSpline.IsInTunnel(i + Step);
                    }

                    if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.NoTurnLane)
                    {
                        rVect = (tVect + new Vector3(RoadSeperation_NoTurn * POS.normalized.z, 0, RoadSeperation_NoTurn * -POS.normalized.x));
                        lVect = (tVect + new Vector3(RoadSeperation_NoTurn * -POS.normalized.z, 0, RoadSeperation_NoTurn * POS.normalized.x));
                    }
                    else if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.TurnLane)
                    {
                        rVect = (tVect + new Vector3(RoadSep1Lane * POS.normalized.z, 0, RoadSep1Lane * -POS.normalized.x));
                        lVect = (tVect + new Vector3(RoadSep1Lane * -POS.normalized.z, 0, RoadSep1Lane * POS.normalized.x));
                    }
                    else if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                    {
                        if (isPastInter)
                        {
                            rVect = (tVect + new Vector3(RoadSep1Lane * POS.normalized.z, 0, RoadSep1Lane * -POS.normalized.x));
                            lVect = (tVect + new Vector3(RoadSep2Lane * -POS.normalized.z, 0, RoadSep2Lane * POS.normalized.x));
                        }
                        else
                        {
                            rVect = (tVect + new Vector3(RoadSep2Lane * POS.normalized.z, 0, RoadSep2Lane * -POS.normalized.x));
                            lVect = (tVect + new Vector3(RoadSep1Lane * -POS.normalized.z, 0, RoadSep1Lane * POS.normalized.x));
                        }
                    }
                    else
                    {
                        rVect = (tVect + new Vector3(RoadSeperation * POS.normalized.z, 0, RoadSeperation * -POS.normalized.x));
                        lVect = (tVect + new Vector3(RoadSeperation * -POS.normalized.z, 0, RoadSeperation * POS.normalized.x));
                    }

                    if (tIntStrength >= 1f)
                    {
                        tVect.y -= tInterSubtract;
                        tLastInterHeight = tVect.y;
                        rVect.y -= tInterSubtract;
                        lVect.y -= tInterSubtract;
                    }
                    else
                    {
                        if (!RootUtils.IsApproximately(tIntStrength, 0f, 0.001f))
                        {
                            tVect.y = (tIntStrength * tIntHeight) + ((1 - tIntStrength) * tVect.y);
                        }
                        tIntStrength_temp = _road.spline.IntersectionStrength(ref rVect, ref tIntHeight, ref roadIntersection, ref isPastInter, ref i, ref xNode);
                        if (!RootUtils.IsApproximately(tIntStrength_temp, 0f, 0.001f))
                        {
                            rVect.y = (tIntStrength_temp * tIntHeight) + ((1 - tIntStrength_temp) * rVect.y);
                            ShoulderR_lVect = rVect;
                        }
                    }

                    //Add bounds for later removal:
                    Construction2DRect vRect = null;
                    if (!isBridge && !isTunnel && isMaxIntersection && isWasPrevMaxInter)
                    {
                        bool isGoAhead = true;
                        if (xNode.isEndPoint)
                        {
                            if (xNode.idOnSpline == 1)
                            {
                                if (i < xNode.time)
                                {
                                    isGoAhead = false;
                                }
                            }
                            else
                            {
                                if (i > xNode.time)
                                {
                                    isGoAhead = false;
                                }
                            }
                        }
                        //Get this and prev lvect rvect rects:
                        if (Vector3.Distance(xNode.pos, tVect) < (3f * RoadWidth) && isGoAhead)
                        {
                            if (roadIntersection.isFlipped && !isFirstInterNode)
                            {
                                vRect = new Construction2DRect(
                                    new Vector2(rVect.x, rVect.z),
                                    new Vector2(lVect.x, lVect.z),
                                    new Vector2(rVect_Prev.x, rVect_Prev.z),
                                    new Vector2(lVect_Prev.x, lVect_Prev.z),
                                    tLastInterHeight
                                    );
                            }
                            else
                            {
                                vRect = new Construction2DRect(
                                   new Vector2(lVect.x, lVect.z),
                                   new Vector2(rVect.x, rVect.z),
                                   new Vector2(lVect_Prev.x, lVect_Prev.z),
                                   new Vector2(rVect_Prev.x, rVect_Prev.z),
                                   tLastInterHeight
                                   );
                            }
                            //GameObject tObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            //tObj.transform.position = lVect;
                            //tObj.transform.localScale = new Vector3(0.2f,20f,0.2f);
                            //tObj.transform.name = "temp22";
                            //							
                            //tObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            //tObj.transform.position = rVect;
                            //tObj.transform.localScale = new Vector3(0.2f,20f,0.2f);
                            //tObj.transform.name = "temp22";

                            _RCS.tIntersectionBounds.Add(vRect);
                        }
                    }
                }

                isWasPrevMaxInter = isMaxIntersection;
                tVect_Prev = tVect;
                rVect_Prev = rVect;
                lVect_Prev = lVect;
                ShoulderR_PrevLVect = ShoulderR_lVect;
                ShoulderL_PrevRVect = ShoulderL_rVect;
                //ShoulderR_PrevRVect3 = ShoulderR_PrevRVect2;
                //ShoulderL_PrevLVect3 = ShoulderL_PrevLVect2;
                //ShoulderR_PrevRVect2 = ShoulderR_PrevRVect;
                //ShoulderL_PrevLVect2 = ShoulderL_PrevLVect;
                ShoulderR_PrevRVect = ShoulderR_rVect;
                ShoulderL_PrevLVect = ShoulderL_lVect;
                RampR_PrevR = RampR_R;
                RampR_PrevL = RampR_L;
                RampL_PrevR = RampL_R;
                RampL_PrevL = RampL_L;
                //i+=Step; 
            }
        }
        #endregion


        #region "Intersection Prelim Finalization"		
        public static void RoadJobPrelimFinalizeInter(ref Road _road)
        {
            int mCount = _road.spline.GetNodeCount();
            SplineN tNode;
            for (int index = 0; index < mCount; index++)
            {
                tNode = _road.spline.nodes[index];
                if (tNode.isIntersection)
                {
                    Inter_OrganizeVertices(ref tNode, ref _road);
                    tNode.intersectionConstruction.Nullify();
                    tNode.intersectionConstruction = null;
                }
            }
        }


        private static bool InterOrganizeVerticesMatchEdges(ref List<Vector3> _list1, ref List<Vector3> _list2, bool _isSkip1 = false, bool _isSkippingFirstListOne = false, bool _isSkippingBoth = false)
        {
            List<Vector3> PrimaryList;
            List<Vector3> SecondaryList;

            List<Vector3> tList1New;
            List<Vector3> tList2New;

            if (_isSkip1)
            {
                if (_isSkippingBoth)
                {
                    tList1New = new List<Vector3>();
                    tList2New = new List<Vector3>();
                    for (int index = 1; index < _list1.Count; index++)
                    {
                        tList1New.Add(_list1[index]);
                    }
                    for (int index = 1; index < _list2.Count; index++)
                    {
                        tList2New.Add(_list2[index]);
                    }
                }
                else
                {
                    if (_isSkippingFirstListOne)
                    {
                        tList1New = new List<Vector3>();
                        for (int index = 1; index < _list1.Count; index++)
                        {
                            tList1New.Add(_list1[index]);
                        }
                        tList2New = _list2;
                    }
                    else
                    {
                        tList2New = new List<Vector3>();
                        for (int index = 1; index < _list2.Count; index++)
                        {
                            tList2New.Add(_list2[index]);
                        }
                        tList1New = _list1;
                    }
                }
            }
            else
            {
                tList1New = _list1;
                tList2New = _list2;
            }

            int tList1Count = tList1New.Count;
            int tList2Count = tList2New.Count;
            if (tList1Count == tList2Count)
            {
                return false;
            }

            if (tList1Count > tList2Count)
            {
                PrimaryList = tList1New;
                SecondaryList = tList2New;
            }
            else
            {
                PrimaryList = tList2New;
                SecondaryList = tList1New;
            }

            if (SecondaryList == null || SecondaryList.Count == 0)
            {
                return true;
            }
            SecondaryList.Clear();
            SecondaryList = null;
            SecondaryList = new List<Vector3>();
            for (int index = 0; index < PrimaryList.Count; index++)
            {
                SecondaryList.Add(PrimaryList[index]);
            }


            if (tList1Count > tList2Count)
            {
                _list2 = SecondaryList;
            }
            else
            {
                _list1 = SecondaryList;
            }

            return false;
        }


        private static void InterOrganizeVerticesMatchShoulder(ref List<Vector3> _shoulderList, ref List<Vector3> _toMatch, int _startI, ref Vector3 _startVec, ref Vector3 _endVect, float _height, bool _isF = false)
        {
            List<Vector3> BackupList = new List<Vector3>();
            for (int index = 0; index < _toMatch.Count; index++)
            {
                BackupList.Add(_toMatch[index]);
            }
            Vector2 t2D = default(Vector2);
            Vector2 t2D_Start = ConvertVect3ToVect2(_startVec);
            Vector2 t2D_End = ConvertVect3ToVect2(_endVect);
            int RealStartID = -1;
            _startI = _startI - 30;
            if (_startI < 0)
            {
                _startI = 0;
            }
            for (int index = _startI; index < _shoulderList.Count; index++)
            {
                t2D = ConvertVect3ToVect2(_shoulderList[index]);
                //if(t2D.x > 745f && t2D.x < 755f && t2D.y > 1240f && t2D.y < 1250f)
                //{
                //	int testInteger = 1;	
                //}
                if (t2D == t2D_Start)
                {
                    //if(tShoulderList[i] == StartVec){
                    RealStartID = index;
                    break;
                }
            }

            _toMatch.Clear();
            _toMatch = null;
            _toMatch = new List<Vector3>();

            int spamcounter = 0;
            bool bBackup = false;
            if (RealStartID == -1)
            {
                bBackup = true;
            }

            if (!bBackup)
            {
                if (_isF)
                {
                    for (int index = RealStartID; index > 0; index -= 8)
                    {
                        t2D = ConvertVect3ToVect2(_shoulderList[index]);
                        _toMatch.Add(_shoulderList[index]);
                        if (t2D == t2D_End)
                        {
                            //if(tShoulderList[i] == EndVect){
                            break;
                        }
                        spamcounter += 1;
                        if (spamcounter > 100)
                        {
                            bBackup = true;
                            break;
                        }
                    }
                }
                else
                {
                    for (int index = RealStartID; index < _shoulderList.Count; index += 8)
                    {
                        t2D = ConvertVect3ToVect2(_shoulderList[index]);
                        _toMatch.Add(_shoulderList[index]);
                        if (t2D == t2D_End)
                        {
                            //if(tShoulderList[i] == EndVect){
                            break;
                        }
                        spamcounter += 1;
                        if (spamcounter > 100)
                        {
                            bBackup = true;
                            break;
                        }
                    }
                }
            }
            ////			
            //			if(!bBackup){
            //				for(int i=0;i<tToMatch.Count;i++){
            //					tToMatch[i] = new Vector3(tToMatch[i].x,tHeight,tToMatch[i].z);
            //				}
            //			}
            //			
            //			//Backup if above fails:
            //			if(bBackup){
            //				tToMatch.Clear();
            //				tToMatch = new List<Vector3>();
            //				for(int i=0;i<BackupList.Count;i++){
            //					tToMatch.Add(BackupList[i]);
            //				}
            //			}
        }


        private static void Inter_OrganizeVertices(ref SplineN _node, ref Road _road)
        {
            iConstructionMaker iCon = _node.intersectionConstruction;
            RoadIntersection roadIntersection = _node.intersection;

            //Skipping (3 ways):
            bool isSkipF = false;
            if (iCon.iFLane0L.Count == 0)
            {
                isSkipF = true;
            }
            bool bSkipB = false;
            if (iCon.iBLane0L.Count == 0)
            {
                bSkipB = true;
            }

            //Is primary node and is first node on a spline, meaning t junction: It does not have a B:
            if (_node.idOnSpline == 0 && string.CompareOrdinal(roadIntersection.node1uID, _node.uID) == 0)
            {
                bSkipB = true;
            }
            //Is primary node and is last node on a spline, meaning t junction: It does not have a F:
            if (_node.idOnSpline == (_node.spline.GetNodeCount() - 1) && string.CompareOrdinal(roadIntersection.node1uID, _node.uID) == 0)
            {
                isSkipF = true;
            }

            //Other node is t junction end node, meaning now we figure out which side we're on
            if (_node.intersectionOtherNode.idOnSpline == 0 || _node.idOnSpline == (_node.spline.GetNodeCount() - 1))
            {

            }

            //Reverse all fronts:
            if (!isSkipF)
            {
                iCon.iFLane0L.Reverse();
                iCon.iFLane0R.Reverse();

                iCon.iFLane1L.Reverse();
                iCon.iFLane2L.Reverse();
                iCon.iFLane3L.Reverse();
                iCon.iFLane1R.Reverse();
                iCon.iFLane2R.Reverse();
                iCon.iFLane3R.Reverse();

                if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                {
                    iCon.shoulderStartFR = iCon.iFLane0L[0];
                    iCon.shoulderStartFL = iCon.iFLane3R[0];
                }
                else if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.TurnLane)
                {
                    iCon.shoulderStartFR = iCon.iFLane0L[0];
                    iCon.shoulderStartFL = iCon.iFLane2R[0];
                }
                else
                {
                    iCon.shoulderStartFR = iCon.iFLane0L[0];
                    iCon.shoulderStartFL = iCon.iFLane1R[0];
                }
            }

            if (!bSkipB)
            {
                if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                {
                    iCon.shoulderEndBL = iCon.iBLane0L[iCon.iBLane0L.Count - 1];
                    iCon.shoulderEndBR = iCon.iBLane3R[iCon.iBLane3R.Count - 1];
                }
                else if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.TurnLane)
                {
                    iCon.shoulderEndBL = iCon.iBLane0L[iCon.iBLane0L.Count - 1];
                    iCon.shoulderEndBR = iCon.iBLane2R[iCon.iBLane2R.Count - 1];
                }
                else
                {
                    iCon.shoulderEndBL = iCon.iBLane0L[iCon.iBLane0L.Count - 1];
                    iCon.shoulderEndBR = iCon.iBLane1R[iCon.iBLane1R.Count - 1];
                }
            }

            if (!bSkipB)
            {
                InterOrganizeVerticesMatchShoulder(ref _road.RCS.ShoulderL_Vectors, ref iCon.iBLane0L, iCon.shoulderBLStartIndex, ref iCon.shoulderStartBL, ref iCon.shoulderEndBL, roadIntersection.height);
                if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                {
                    InterOrganizeVerticesMatchShoulder(ref _road.RCS.ShoulderR_Vectors, ref iCon.iBLane3R, iCon.shoulderBRStartIndex, ref iCon.shoulderStartBR, ref iCon.shoulderEndBR, roadIntersection.height);
                }
                else if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.TurnLane)
                {
                    InterOrganizeVerticesMatchShoulder(ref _road.RCS.ShoulderR_Vectors, ref iCon.iBLane2R, iCon.shoulderBRStartIndex, ref iCon.shoulderStartBR, ref iCon.shoulderEndBR, roadIntersection.height);
                }
                else
                {
                    InterOrganizeVerticesMatchShoulder(ref _road.RCS.ShoulderR_Vectors, ref iCon.iBLane1R, iCon.shoulderBRStartIndex, ref iCon.shoulderStartBR, ref iCon.shoulderEndBR, roadIntersection.height);
                }
            }

            if (!isSkipF)
            {
                InterOrganizeVerticesMatchShoulder(ref _road.RCS.ShoulderR_Vectors, ref iCon.iFLane0L, iCon.shoulderFRStartIndex, ref iCon.shoulderStartFR, ref iCon.shoulderEndFR, roadIntersection.height, true);
                if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                {
                    InterOrganizeVerticesMatchShoulder(ref _road.RCS.ShoulderL_Vectors, ref iCon.iFLane3R, iCon.shoulderFLStartIndex, ref iCon.shoulderStartFL, ref iCon.shoulderEndFL, roadIntersection.height, true);
                }
                else if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.TurnLane)
                {
                    InterOrganizeVerticesMatchShoulder(ref _road.RCS.ShoulderL_Vectors, ref iCon.iFLane2R, iCon.shoulderFLStartIndex, ref iCon.shoulderStartFL, ref iCon.shoulderEndFL, roadIntersection.height, true);
                }
                else
                {
                    InterOrganizeVerticesMatchShoulder(ref _road.RCS.ShoulderL_Vectors, ref iCon.iFLane1R, iCon.shoulderFLStartIndex, ref iCon.shoulderStartFL, ref iCon.shoulderEndFL, roadIntersection.height, true);
                }
            }

            bool bError = false;
            string tWarning = "Intersection " + roadIntersection.intersectionName + " in road " + _road.roadName + " at too extreme angle to process this intersection type. Reduce angle or reduce lane count.";

            if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.NoTurnLane)
            {
                if (!bSkipB)
                {
                    bError = InterOrganizeVerticesMatchEdges(ref iCon.iBLane0R, ref iCon.iBLane1L);
                    if (bError)
                    {
                        Debug.Log(tWarning);
                    }
                }
                if (!isSkipF)
                {
                    bError = InterOrganizeVerticesMatchEdges(ref iCon.iFLane0R, ref iCon.iFLane1L);
                    if (bError)
                    {
                        Debug.Log(tWarning);
                    }
                }
            }
            else if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.TurnLane)
            {
                if (!bSkipB)
                {
                    bError = InterOrganizeVerticesMatchEdges(ref iCon.iBLane0R, ref iCon.iBLane1L);
                    if (bError)
                    {
                        Debug.Log(tWarning);
                    }
                }
                if (!isSkipF)
                {
                    bError = InterOrganizeVerticesMatchEdges(ref iCon.iFLane0R, ref iCon.iFLane1L);
                    if (bError)
                    {
                        Debug.Log(tWarning);
                    }
                }

                if (!bSkipB)
                {
                    bError = InterOrganizeVerticesMatchEdges(ref iCon.iBLane1R, ref iCon.iBLane2L);
                    if (bError)
                    {
                        Debug.Log(tWarning);
                    }
                }
                if (!isSkipF)
                {
                    bError = InterOrganizeVerticesMatchEdges(ref iCon.iFLane1R, ref iCon.iFLane2L);
                    if (bError)
                    {
                        Debug.Log(tWarning);
                    }
                }
            }
            else if (roadIntersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
            {
                if (!bSkipB)
                {
                    bError = InterOrganizeVerticesMatchEdges(ref iCon.iBLane0R, ref iCon.iBLane1L);
                    if (bError)
                    {
                        Debug.Log(tWarning);
                    }
                }
                if (!isSkipF)
                {
                    bError = InterOrganizeVerticesMatchEdges(ref iCon.iFLane0R, ref iCon.iFLane1L);
                    if (bError)
                    {
                        Debug.Log(tWarning);
                    }
                }

                if (!bSkipB)
                {
                    bError = InterOrganizeVerticesMatchEdges(ref iCon.iBLane1R, ref iCon.iBLane2L, true, true);
                    if (bError)
                    {
                        Debug.Log(tWarning);
                    }
                }
                if (!isSkipF)
                {
                    bError = InterOrganizeVerticesMatchEdges(ref iCon.iFLane1R, ref iCon.iFLane2L, true, true);
                    if (bError)
                    {
                        Debug.Log(tWarning);
                    }
                }

                //				if(!bSkipB){ bError = Inter_OrganizeVerticesMatchEdges(ref iCon.iBLane2R, ref iCon.iBLane3L,true,false); if(bError){ Debug.Log(tWarning); } }
                //				if(!bSkipF){ bError = Inter_OrganizeVerticesMatchEdges(ref iCon.iFLane2R, ref iCon.iFLane3L,true,false); if(bError){ Debug.Log(tWarning); } }
            }

            //Back main plate left:
            int mCount = -1;
            if (!bSkipB)
            {
                mCount = iCon.iBLane0L.Count;
                for (int m = 0; m < mCount; m++)
                {
                    iCon.iBMainPlateL.Add(iCon.iBLane0L[m]);
                }
            }
            //Front main plate left:
            if (!isSkipF)
            {
                mCount = iCon.iFLane0L.Count;
                for (int m = 0; m < mCount; m++)
                {
                    iCon.iFMainPlateL.Add(iCon.iFLane0L[m]);
                }
            }

            //Back main plate right:
            if (!bSkipB)
            {
                if (_node.intersection.roadType == RoadIntersection.RoadTypeEnum.NoTurnLane)
                {
                    mCount = iCon.iBLane1R.Count;
                    for (int m = 0; m < mCount; m++)
                    {
                        iCon.iBMainPlateR.Add(iCon.iBLane1R[m]);
                    }
                }
                else if (_node.intersection.roadType == RoadIntersection.RoadTypeEnum.TurnLane)
                {
                    mCount = iCon.iBLane2R.Count;
                    for (int m = 0; m < mCount; m++)
                    {
                        iCon.iBMainPlateR.Add(iCon.iBLane2R[m]);
                    }
                }
                else if (_node.intersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                {
                    mCount = iCon.iBLane3R.Count;
                    for (int m = 0; m < mCount; m++)
                    {
                        iCon.iBMainPlateR.Add(iCon.iBLane3R[m]);
                    }
                }
            }

            //Front main plate right:
            if (!isSkipF)
            {
                if (_node.intersection.roadType == RoadIntersection.RoadTypeEnum.NoTurnLane)
                {
                    mCount = iCon.iFLane1R.Count;
                    for (int m = 0; m < mCount; m++)
                    {
                        iCon.iFMainPlateR.Add(iCon.iFLane1R[m]);
                    }
                }
                else if (_node.intersection.roadType == RoadIntersection.RoadTypeEnum.TurnLane)
                {
                    mCount = iCon.iFLane2R.Count;
                    for (int m = 0; m < mCount; m++)
                    {
                        iCon.iFMainPlateR.Add(iCon.iFLane2R[m]);
                    }
                }
                else if (_node.intersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                {
                    mCount = iCon.iFLane3R.Count;
                    for (int m = 0; m < mCount; m++)
                    {
                        iCon.iFMainPlateR.Add(iCon.iFLane3R[m]);
                    }
                }
            }

            mCount = _road.RCS.RoadVectors.Count;
            //			float mDistance = 0.05f;
            Vector3 tVect = default(Vector3);

            bool biBLane0L = (iCon.iBLane0L.Count > 0);
            bool biBLane0R = (iCon.iBLane0R.Count > 0);
            bool biBMainPlateL = (iCon.iBMainPlateL.Count > 0);
            bool biBMainPlateR = (iCon.iBMainPlateR.Count > 0);
            bool biFLane0L = (iCon.iFLane0L.Count > 0);
            bool biFLane0R = (iCon.iFLane0R.Count > 0);
            bool biFMainPlateL = (iCon.iFMainPlateL.Count > 0);
            bool biFMainPlateR = (iCon.iFMainPlateR.Count > 0);
            bool biBLane2L = (iCon.iBLane2L.Count > 0);
            bool biBLane2R = (iCon.iBLane2R.Count > 0);
            bool biFLane2L = (iCon.iFLane2L.Count > 0);
            bool biFLane2R = (iCon.iFLane2R.Count > 0);
            bool biBLane3L = (iCon.iBLane3L.Count > 0);
            bool biBLane3R = (iCon.iBLane3R.Count > 0);
            bool biFLane3L = (iCon.iFLane3L.Count > 0);
            bool biFLane3R = (iCon.iFLane3R.Count > 0);

            mCount = _road.RCS.RoadVectors.Count;
            int cCount = _road.spline.GetNodeCount();
            int tStartI = 0;
            int tEndI = mCount;
            //Start and end the next loop after this one later for opt:
            if (cCount > 2)
            {
                if (!_road.spline.nodes[0].isIntersection && !_road.spline.nodes[1].isIntersection)
                {
                    for (int i = 2; i < cCount; i++)
                    {
                        if (_road.spline.nodes[i].isIntersection)
                        {
                            if (i - 2 >= 1)
                            {
                                tStartI = (int)(_road.spline.nodes[i - 2].time * mCount);
                            }
                            break;
                        }
                    }
                }
            }
            if (cCount > 3)
            {
                if (!_road.spline.nodes[cCount - 1].isIntersection && !_road.spline.nodes[cCount - 2].isIntersection)
                {
                    for (int i = (cCount - 3); i >= 0; i--)
                    {
                        if (_road.spline.nodes[i].isIntersection)
                        {
                            if (i + 2 < cCount)
                            {
                                tEndI = (int)(_road.spline.nodes[i + 2].time * mCount);
                            }
                            break;
                        }
                    }
                }
            }

            if (tStartI > 0)
            {
                if (tStartI % 2 != 0)
                {
                    tStartI += 1;
                }
            }
            if (tStartI > mCount)
            {
                tStartI = mCount - 4;
            }
            if (tStartI < 0)
            {
                tStartI = 0;
            }
            if (tEndI < mCount)
            {
                if (tEndI % 2 != 0)
                {
                    tEndI += 1;
                }
            }
            if (tEndI > mCount)
            {
                tEndI = mCount - 4;
            }
            if (tEndI < 0)
            {
                tEndI = 0;
            }

            for (int i = tStartI; i < tEndI; i += 2)
            {
                tVect = _road.RCS.RoadVectors[i];
                for (int j = 0; j < 1; j++)
                {
                    if (biBLane0L && Vector3.SqrMagnitude(tVect - iCon.iBLane0L[j]) < 0.01f && !bSkipB)
                    {
                        iCon.iBLane0L[j] = tVect;
                    }
                    if (biBMainPlateL && Vector3.SqrMagnitude(tVect - iCon.iBMainPlateL[j]) < 0.01f && !bSkipB)
                    {
                        iCon.iBMainPlateL[j] = tVect;
                    }
                    if (biBMainPlateR && Vector3.SqrMagnitude(tVect - iCon.iBMainPlateR[j]) < 0.01f && !bSkipB)
                    {
                        iCon.iBMainPlateR[j] = tVect;
                    }
                    if (biFLane0L && Vector3.SqrMagnitude(tVect - iCon.iFLane0L[j]) < 0.01f && !isSkipF)
                    {
                        iCon.iFLane0L[j] = tVect;
                    }
                    if (biFMainPlateL && Vector3.SqrMagnitude(tVect - iCon.iFMainPlateL[j]) < 0.01f && !isSkipF)
                    {
                        iCon.iFMainPlateL[j] = tVect;
                    }
                    if (biFMainPlateR && Vector3.SqrMagnitude(tVect - iCon.iFMainPlateR[j]) < 0.01f && !isSkipF)
                    {
                        iCon.iFMainPlateR[j] = tVect;
                    }
                    if (_node.intersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                    {
                        if (biBLane3L && Vector3.SqrMagnitude(tVect - iCon.iBLane3L[j]) < 0.01f && !bSkipB)
                        {
                            iCon.iBLane3L[j] = tVect;
                        }
                        if (biBLane3R && Vector3.SqrMagnitude(tVect - iCon.iBLane3R[j]) < 0.01f && !bSkipB)
                        {
                            iCon.iBLane3R[j] = tVect;
                        }
                        if (biFLane3L && Vector3.SqrMagnitude(tVect - iCon.iFLane3L[j]) < 0.01f && !isSkipF)
                        {
                            iCon.iFLane3L[j] = tVect;
                        }
                        if (biFLane3R && Vector3.SqrMagnitude(tVect - iCon.iFLane3R[j]) < 0.01f && !isSkipF)
                        {
                            iCon.iFLane3R[j] = tVect;
                        }
                    }
                    else if (_node.intersection.roadType == RoadIntersection.RoadTypeEnum.TurnLane)
                    {
                        if (biBLane2L && Vector3.SqrMagnitude(tVect - iCon.iBLane2L[j]) < 0.01f && !bSkipB)
                        {
                            iCon.iBLane2L[j] = tVect;
                        }
                        if (biBLane2R && Vector3.SqrMagnitude(tVect - iCon.iBLane2R[j]) < 0.01f && !bSkipB)
                        {
                            iCon.iBLane2R[j] = tVect;
                        }
                        if (biFLane2L && Vector3.SqrMagnitude(tVect - iCon.iFLane2L[j]) < 0.01f && !isSkipF)
                        {
                            iCon.iFLane2L[j] = tVect;
                        }
                        if (biFLane2R && Vector3.SqrMagnitude(tVect - iCon.iFLane2R[j]) < 0.01f && !isSkipF)
                        {
                            iCon.iFLane2R[j] = tVect;
                        }
                    }
                }
            }

            //			float b0 = -1f;
            //			float f0 = -1f;
            //			
            //			if(!bSkipB){ b0 = iCon.iBMainPlateL[0].y; }
            //			if(!bSkipF){ f0 = iCon.iFMainPlateL[0].y; }
            //			
            //			if(iCon.iBLane0R == null || iCon.iBLane0R.Count == 0){
            //				bSkipB = true;	
            //			}
            if (iCon.iBMainPlateR == null || iCon.iBMainPlateR.Count == 0)
            {
                bSkipB = true;
            }
            if (iCon.iBMainPlateL == null || iCon.iBMainPlateL.Count == 0)
            {
                bSkipB = true;
            }

            if (!bSkipB)
            {
                iCon.iBLane0R[0] = ((iCon.iBMainPlateR[0] - iCon.iBMainPlateL[0]) * 0.5f + iCon.iBMainPlateL[0]);
            }
            if (!isSkipF)
            {
                iCon.iFLane0R[0] = ((iCon.iFMainPlateR[0] - iCon.iFMainPlateL[0]) * 0.5f + iCon.iFMainPlateL[0]);
            }

            //			if(tNode.roadIntersection.rType != RoadIntersection.RoadTypeEnum.NoTurnLane){ 
            if (!bSkipB)
            {
                iCon.iBLane1L[0] = iCon.iBLane0R[0];
                iCon.iBLane1R[0] = new Vector3(iCon.iBLane1R[0].x, iCon.iBLane1L[0].y, iCon.iBLane1R[0].z);
            }

            if (!isSkipF)
            {
                iCon.iFLane1L[0] = iCon.iFLane0R[0];
                iCon.iFLane1R[0] = new Vector3(iCon.iFLane1R[0].x, iCon.iFLane1L[0].y, iCon.iFLane1R[0].z);
            }
            //			}

            if (_node.intersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
            {
                if (!bSkipB)
                {
                    iCon.iBLane3L[0] = new Vector3(iCon.iBLane3L[0].x, iCon.iBLane3R[0].y, iCon.iBLane3L[0].z);
                }
                if (!isSkipF)
                {
                    iCon.iFLane3L[0] = new Vector3(iCon.iFLane3L[0].x, iCon.iFLane3R[0].y, iCon.iFLane3L[0].z);
                }
            }
            else if (_node.intersection.roadType == RoadIntersection.RoadTypeEnum.TurnLane)
            {
                if (!bSkipB)
                {
                    iCon.iBLane2L[0] = new Vector3(iCon.iBLane2L[0].x, iCon.iBLane2R[0].y, iCon.iBLane2L[0].z);
                }
                if (!isSkipF)
                {
                    iCon.iFLane2L[0] = new Vector3(iCon.iFLane2L[0].x, iCon.iFLane2R[0].y, iCon.iFLane2L[0].z);
                }
            }

            List<Vector3> iBLane0 = null;
            List<Vector3> iBLane1 = null;
            List<Vector3> iBLane2 = null;
            List<Vector3> iBLane3 = null;
            if (!bSkipB)
            {
                iBLane0 = InterVertices(iCon.iBLane0L, iCon.iBLane0R, _node.intersection.height);
                iBLane1 = InterVertices(iCon.iBLane1L, iCon.iBLane1R, _node.intersection.height);
                if (_node.intersection.roadType != RoadIntersection.RoadTypeEnum.NoTurnLane)
                {
                    iBLane2 = InterVertices(iCon.iBLane2L, iCon.iBLane2R, _node.intersection.height);
                }
                if (_node.intersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                {
                    iBLane3 = InterVertices(iCon.iBLane3L, iCon.iBLane3R, _node.intersection.height);
                }
            }

            //Front lanes:
            List<Vector3> iFLane0 = null;
            List<Vector3> iFLane1 = null;
            List<Vector3> iFLane2 = null;
            List<Vector3> iFLane3 = null;
            if (!isSkipF)
            {
                iFLane0 = InterVertices(iCon.iFLane0L, iCon.iFLane0R, _node.intersection.height);
                iFLane1 = InterVertices(iCon.iFLane1L, iCon.iFLane1R, _node.intersection.height);
                if (_node.intersection.roadType != RoadIntersection.RoadTypeEnum.NoTurnLane)
                {
                    iFLane2 = InterVertices(iCon.iFLane2L, iCon.iFLane2R, _node.intersection.height);
                }
                if (_node.intersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                {
                    iFLane3 = InterVertices(iCon.iFLane3L, iCon.iFLane3R, _node.intersection.height);
                }
            }

            //Main plates:
            List<Vector3> iBMainPlate = null;
            List<Vector3> iFMainPlate = null;
            if (!bSkipB)
            {
                iBMainPlate = InterVertices(iCon.iBMainPlateL, iCon.iBMainPlateR, _node.intersection.height);
            }
            if (!isSkipF)
            {
                iFMainPlate = InterVertices(iCon.iFMainPlateL, iCon.iFMainPlateR, _node.intersection.height);
            }
            //			//Marker plates:
            //			List<Vector3> iBMarkerPlate = InterVertices(iCon.iBMarkerPlateL,iCon.iBMarkerPlateR, tNode.roadIntersection.Height);
            //			List<Vector3> iFMarkerPlate = InterVertices(iCon.iFMarkerPlateL,iCon.iFMarkerPlateR, tNode.roadIntersection.Height);
            //			
            //Now add these to RCS:
            if (!bSkipB)
            {
                _road.RCS.iBLane0s.Add(iBLane0.ToArray());
                _road.RCS.iBLane0s_tID.Add(roadIntersection);
                _road.RCS.iBLane0s_nID.Add(_node);
                _road.RCS.iBLane1s.Add(iBLane1.ToArray());
                _road.RCS.iBLane1s_tID.Add(roadIntersection);
                _road.RCS.iBLane1s_nID.Add(_node);
                if (_node.intersection.roadType != RoadIntersection.RoadTypeEnum.NoTurnLane)
                {
                    if (iBLane2 != null)
                    {
                        _road.RCS.iBLane2s.Add(iBLane2.ToArray());
                        _road.RCS.iBLane2s_tID.Add(roadIntersection);
                        _road.RCS.iBLane2s_nID.Add(_node);
                    }
                }
                if (_node.intersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                {
                    _road.RCS.iBLane3s.Add(iBLane3.ToArray());
                    _road.RCS.iBLane3s_tID.Add(roadIntersection);
                    _road.RCS.iBLane3s_nID.Add(_node);
                }
            }
            //Front lanes:
            if (!isSkipF)
            {
                _road.RCS.iFLane0s.Add(iFLane0.ToArray());
                _road.RCS.iFLane0s_tID.Add(roadIntersection);
                _road.RCS.iFLane0s_nID.Add(_node);
                _road.RCS.iFLane1s.Add(iFLane1.ToArray());
                _road.RCS.iFLane1s_tID.Add(roadIntersection);
                _road.RCS.iFLane1s_nID.Add(_node);
                if (_node.intersection.roadType != RoadIntersection.RoadTypeEnum.NoTurnLane)
                {
                    _road.RCS.iFLane2s.Add(iFLane2.ToArray());
                    _road.RCS.iFLane2s_tID.Add(roadIntersection);
                    _road.RCS.iFLane2s_nID.Add(_node);
                }
                if (_node.intersection.roadType == RoadIntersection.RoadTypeEnum.BothTurnLanes)
                {
                    _road.RCS.iFLane3s.Add(iFLane3.ToArray());
                    _road.RCS.iFLane3s_tID.Add(roadIntersection);
                    _road.RCS.iFLane3s_nID.Add(_node);
                }
            }
            //Main plates:
            if (iBMainPlate != null && !bSkipB)
            {
                _road.RCS.iBMainPlates.Add(iBMainPlate.ToArray());
                _road.RCS.iBMainPlates_tID.Add(roadIntersection);
                _road.RCS.iBMainPlates_nID.Add(_node);
            }
            if (iFMainPlate != null && !isSkipF)
            {
                _road.RCS.iFMainPlates.Add(iFMainPlate.ToArray());
                _road.RCS.iFMainPlates_tID.Add(roadIntersection);
                _road.RCS.iFMainPlates_nID.Add(_node);
            }
            //			//Marker plates:
            //			tRoad.RCS.iBMarkerPlates.Add(iBMarkerPlate.ToArray());
            //			tRoad.RCS.iFMarkerPlates.Add(iFMarkerPlate.ToArray());
            //			tRoad.RCS.IntersectionTypes.Add((int)tNode.roadIntersection.rType);

            if (_node.intersection.roadType != RoadIntersection.RoadTypeEnum.NoTurnLane)
            {
                if (!bSkipB)
                {
                    _road.RCS.iBLane1s_IsMiddleLane.Add(true);
                }
                if (!isSkipF)
                {
                    _road.RCS.iFLane1s_IsMiddleLane.Add(true);
                }
            }
            else
            {
                if (!bSkipB)
                {
                    _road.RCS.iBLane1s_IsMiddleLane.Add(false);
                }
                if (!isSkipF)
                {
                    _road.RCS.iFLane1s_IsMiddleLane.Add(false);
                }
            }
        }


        private static List<Vector3> InterVertices(List<Vector3> _left, List<Vector3> _right, float _height)
        {
            if (_left.Count == 0 || _right.Count == 0)
            {
                return null;
            }

            List<Vector3> tList = new List<Vector3>();
            int tCountL = _left.Count;
            int tCountR = _right.Count;

            while (tCountL < tCountR)
            {
                _left.Add(_left[tCountL - 1]);
                tCountL = _left.Count;
            }

            while (tCountR < tCountL)
            {
                _right.Add(_right[tCountR - 1]);
                tCountR = _right.Count;
            }

            int tCount = Mathf.Max(tCountL, tCountR);
            for (int i = 0; i < tCount; i++)
            {
                tList.Add(_left[i]);
                tList.Add(_left[i]);
                tList.Add(_right[i]);
                tList.Add(_right[i]);
            }
            return tList;
        }
        #endregion


        /// <summary> Returns true if _vect1 and _vect2 are close to each other </summary>
        private static bool IsVectorSimilar(ref Vector3 _vect1, Vector3 _vect2)
        {
            return Vector3.SqrMagnitude(_vect1 - _vect2) < 0.01f;
        }


        /// <summary> Returns a new Vector2 from _vect.x, _vect.z </summary>
        private static Vector2 ConvertVect3ToVect2(Vector3 _vect)
        {
            return new Vector2(_vect.x, _vect.z);
        }
    }
}
