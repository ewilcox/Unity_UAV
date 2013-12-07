using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UAVUnitTest
{
    class Program
    {
        static void Main(string[] args)
        {
            const int tests = 4;
            const float floatCompareThresh = 0.05F;
            int passed = 0;
            bool fail = false;

            Console.WriteLine("Initializing test UAV MonoBehavior (BlockCrowd2)...");
            BlockCrowd2 bc2 = new BlockCrowd2();
            bc2.Start();
            //TODO: check BC2 init success, if any?

            //TODO: test LocalizationPercept
            /*
            UnitTestHokuyo hSens = new UnitTestHokuyo();
            LocalizationPercept_Hokuyo hPS = new LocalizationPercept_Hokuyo(bc2, hSens);
            List<LocPSTest> hTests = new List<LocPSTest>();
            //TODO: hTests.Add(new LocPSTest());
            fail = false;
            foreach (LocPSTest t in hTests)
            {
                //need to have coordinates for some number of UAVs detected in a given test case
                //(as well as the "IMU" sensor input to say where 'this' UAV is)
                //push into sens, then:
                hPS.Update();
                //then, check ps outputs LeftWall, RightWall, Ceiling, Floor, 
                //as well as the list of other UAV percepts from 'this' UAV
            }
            if (!fail)
            {
                passed++;
                Console.WriteLine("Passed: LocalizationPercept_Hokuyo");
            }*/

            //TODO: test CrowdPercept
            //will now depend upon LocalizationPercept_Hokuyo to filter out some crowds "for" other agents to "forage"...
            /*
            UnitTestCrowdDetectionSensor cSens = new UnitTestCrowdDetectionSensor();
            CrowdPercept cPS = new CrowdPercept(bc2, cSens);
            List<CrowdPSTest> cTests = new List<CrowdPSTest>();
            //TODO: cTests.Add(new CrowdPSTest());
            fail = false;
            foreach (CrowdPSTest t in cTests)
            {
                //need to have coordinates for some number of UAVs detected in a given test case
                //(as well as the "IMU" sensor input to say where 'this' UAV is)
                //push into sens, then:
                cPS.Update();
                //then, check ps outputs LeftWall, RightWall, Ceiling, Floor, 
                //as well as the list of other UAV percepts from 'this' UAV
            }
            if (!fail)
            {
                passed++;
                Console.WriteLine("Passed: CrowdPercept");
            }*/

            //test PerpendicularExponentialIncrease
            FloatRefWrap peiMax = new FloatRefWrap(0);
            FloatRefWrap peiMin = new FloatRefWrap(0);
            PerpendicularExponentialIncrease peiL = new PerpendicularExponentialIncrease(new Vector3(-1, 0), peiMax, peiMin, 1, bc2);
            PerpendicularExponentialIncrease peiR = new PerpendicularExponentialIncrease(new Vector3(1, 0), peiMax, peiMin, 1, bc2);
            PerpendicularExponentialIncrease peiU = new PerpendicularExponentialIncrease(new Vector3(0, 1), peiMax, peiMin, 1, bc2);
            PerpendicularExponentialIncrease peiD = new PerpendicularExponentialIncrease(new Vector3(0, -1), peiMax, peiMin, 1, bc2);
            List<PEITest> peiTests = new List<PEITest>();
            peiTests.Add(new PEITest(0.01329041F, -0.9867096F, 0.9406308F));
            peiTests.Add(new PEITest(0.1256852F, -0.8743148F, 0.5605696F));
            peiTests.Add(new PEITest(0.5254931F, -0.4745069F, 0.08892292F));
            peiTests.Add(new PEITest(0.4340415F, -0.5659585F, 0.135493F));
            peiTests.Add(new PEITest(3.079941F, 2.079941F, 0));
            fail = false;
            foreach (PEITest t in peiTests)
            {
                //taken from expected percepts of distance from surface (max) and field minimum point (min)
                //push into percepts (peimin/peimax), update, and check responses match direction of field
                peiMax.val = t.max;
                peiMin.val = t.min;
                peiL.Update();
                peiR.Update();
                peiU.Update();
                peiD.Update();
                //do responses match, accounting for orientation of field?
                if (peiL.ResponseRotate != Vector3.zero
                    || peiR.ResponseRotate != Vector3.zero
                    || peiU.ResponseRotate != Vector3.zero
                    || peiD.ResponseRotate != Vector3.zero)
                    fail = true;
                if (Math.Abs(peiL.ResponseTranslate.x - (0 - t.response)) > floatCompareThresh
                    || Math.Abs(peiR.ResponseTranslate.x - t.response) > floatCompareThresh
                    || Math.Abs(peiD.ResponseTranslate.y - (0 - t.response)) > floatCompareThresh
                    || Math.Abs(peiU.ResponseTranslate.y - t.response) > floatCompareThresh)
                {
                    Console.WriteLine("Wrong response from PerpendicularExponentialIncrease");
                    fail = true;
                    break;
                }
            }
            if (!fail)
            {
                passed++;
                Console.WriteLine("Passed: PerpendicularExponentialIncrease");
            }

            //TODO: AttractiveExponentialDecrease
            Vector3RefWrap aedPos = new Vector3RefWrap(Vector3.zero);
            AttractiveExponentialDecrease aed = new AttractiveExponentialDecrease(bc2, aedPos, 1, 7.5F * 0.7F, true, true, true);
            List<AEDTest> aedTests = new List<AEDTest>();
            aedTests.Add(new AEDTest(2.384186E-06F, -1.596737F, 0F, 6.696319E-08F, -0.0448466F, 0F));
            aedTests.Add(new AEDTest(-0.3662632F, -1.812747F, -0.2761765F, -0.01135526F, -0.05620061F, -0.008562299F));
            aedTests.Add(new AEDTest(-1.332568F, -2.211033F, -2.077948F, -0.0905683F, -0.1502734F, -0.1412282F));
            aedTests.Add(new AEDTest(-2.333724F, -6.237696F, 0.4753599F, -0.3495217F, -0.9342194F, 0.07119463F));
            aedTests.Add(new AEDTest(2.166004F, -6.237696F, 0.4753599F, 0.3271835F, -0.9422287F, 0.07180501F));
            fail = false;
            foreach (AEDTest t in aedTests)
            {
                //taken from expected percepts of distance from surface (max) and field minimum point (min)
                //push into percepts (peimin/peimax), update, and check responses match direction of field
                aedPos.val.x = t.posX;
                aedPos.val.y = t.posY;
                aedPos.val.z = t.posZ;
                aed.Update();
                //do responses match, accounting for orientation of field?
                if (aed.ResponseRotate != Vector3.zero)
                    fail = true;
                if (Math.Abs(aed.ResponseTranslate.x - t.responseX) > floatCompareThresh
                    || Math.Abs(aed.ResponseTranslate.y - t.responseY) > floatCompareThresh
                    || Math.Abs(aed.ResponseTranslate.z - t.responseZ) > floatCompareThresh)
                {
                    Console.WriteLine("Wrong response from AttractiveExponentialDecrease");
                    fail = true;
                    break;
                }
            }
            if (!fail)
            {
                passed++;
                Console.WriteLine("Passed: AttractiveExponentialDecrease");
            }

            //TODO: RepulsiveExponentialIncrease
            Vector3RefWrap reiPos = new Vector3RefWrap(Vector3.zero);
            RepulsiveExponentialIncrease rei = new RepulsiveExponentialIncrease(bc2, reiPos, 1, 10, true, true, true, 0.5F);
            List<REITest> reiTests = new List<REITest>();
            reiTests.Add(new REITest(-3.050564F, -2.745F, -6.792864F, 0.01251747F, 0.01126364F, 0.02787336F));
            reiTests.Add(new REITest(0.7989649F, -2.745F, -6.792864F, -0.004582152F, 0.01574288F, 0.03895783F));
            reiTests.Add(new REITest(-1.094215F, -2.745F, -2.739611F, 0.05344608F, 0.1340774F, 0.1338142F));
            reiTests.Add(new REITest(0.561788F, -2.745F, -2.739611F, -0.02969579F, 0.1450991F, 0.1448142F));
            reiTests.Add(new REITest(0.6043569F, 2.044585F, -2.739611F, -0.0443087F, -0.1498997F, 0.2008558F));
            reiTests.Add(new REITest(0.1707295F, 0.6454296F, -0.4651489F, -0.1815982F, -0.6865178F, 0.4947604F));
            reiTests.Add(new REITest(-0.5146465F, -0.1194305F, -0.3475056F, 0.7657177F, 0.1776949F, 0.5170367F));
            fail = false;
            foreach (REITest t in reiTests)
            {
                //taken from expected percepts of distance from surface (max) and field minimum point (min)
                //push into percepts (peimin/peimax), update, and check responses match direction of field
                reiPos.val.x = t.posX;
                reiPos.val.y = t.posY;
                reiPos.val.z = t.posZ;
                rei.Update();
                //do responses match, accounting for orientation of field?
                if (rei.ResponseRotate != Vector3.zero)
                    fail = true;
                if (Math.Abs(rei.ResponseTranslate.x - t.responseX) > floatCompareThresh
                    || Math.Abs(rei.ResponseTranslate.y - t.responseY) > floatCompareThresh
                    || Math.Abs(rei.ResponseTranslate.z - t.responseZ) > floatCompareThresh)
                {
                    Console.WriteLine("Wrong response from RepulsiveExponentialIncrease");
                    fail = true;
                    break;
                }
            }
            if (!fail)
            {
                passed++;
                Console.WriteLine("Passed: RepulsiveExponentialIncrease");
            }


            //TODO: Rand2D
            //this one's a little less routine... because it's not deterministic if you will, it depends upon random seeding etc... 
            //so just some behaviors expected over time...
            Vector3RefWrap r2dPos = new Vector3RefWrap(Vector3.zero);
            Rand2D r2d = new Rand2D(bc2, r2dPos, 1,
                //in debug, using DateTime.Now.Ticks, the interval's measured in hundreds of nanoseconds...
                //let it be 250ms between changes... or 500,000,000 ns...
                  250 * 1000000,
                  1);
            fail = false;
            int randTests = 10;
            Vector3 r1 = Vector3.zero;
            for (int t = 0; t < randTests; t++)
            {
                r2d.Update();
                if ( r2d.ResponseRotate != Vector3.zero )
                    fail = true;
                if (t > 0 && Math.Acos(Vector3.Dot(r1, r2d.ResponseRotate)) == 0)
                {
                    Console.WriteLine("Rand2D didn't change directions");
                    fail = true;
                    break;
                }
                r1 = r2d.ResponseTranslate;
                r2d.Update();
                if (r1 != r2d.ResponseTranslate)
                {
                    Console.WriteLine("Rand2D changed too fast");
                    fail = true;
                    break;
                }
                if (Math.Abs(r2d.ResponseTranslate.magnitude - 1) > floatCompareThresh)
                {
                    Console.WriteLine("Rand2D wrong strength...");
                    fail = true;
                    break;
                }
                r2dPos.val.x = float.MaxValue;
                r2d.Update();
                if (r2d.ResponseTranslate.magnitude > floatCompareThresh)
                {
                    Console.WriteLine("Rand2D ignoring exponential decrease");
                    fail = true;
                    break;
                }
                r2dPos.val.x = 0;
                System.Threading.Thread.Sleep(300);
            }
            if (!fail)
            {
                passed++;
                Console.WriteLine("Passed: Rand2D");
            }


            //TODO: KeepHeight

            //TODO: HoldCenter

            //TODO: Follow

            //TODO: AvoidCrowd

            //TODO: ThreateningRand2D

            //TODO: Watching (starting to get into input-parameter combinations more applicable to integration testing...)

            //TODO: Approaching

            //TODO: Threatening

            //TODO: Avoid

            //TODO: BlockCrowd2 (test points?  Possible, maybe... too many integrated parameter combinations, probably)


            Console.WriteLine("Unit Tests Complete; " + (tests - passed) + " of " + tests + " unit tests failed.");
            Console.ReadLine();
        }
    }
}
