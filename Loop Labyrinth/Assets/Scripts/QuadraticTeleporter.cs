using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// QuadraticTeleporter teleports objects on one edge of a 4-sided plane to the opposite edge. It does this by creating 9 clones (phantoms) of a GameObject, and when the real GameObject hits an edge, it is moved to the position as a phantom on the opposite side.
public class QuadraticTeleporter : MonoBehaviour
{
    public List<List<GameObject>> list_of_phantoms_and_their_originals = new List<List<GameObject>>();
    public float x_offset;
    public float z_offset;
    public float y_offset;
    public float height_reference = 1000f;
    public string[] phantom_offset_settings = {"XZ", "Z", "xZ", "X", "O", "x", "Xz", "z", "xz"};

    // Start is called before the first frame update
    void Start()
    {
        update_dimensions();
    }

    // Update is called once per frame
    void Update()
    {
        update_dimensions();
        /*
        012
        345
        678
        */
        // Each phantom is associated with a certain index depending on their cardinal direction.

        GameObject[] list_of_clonable_objects = GameObject.FindGameObjectsWithTag("Clonable"); /* This is an array of everything that can teleport. 
                                                                                                So, if there are things that can roll around the 
                                                                                                board and you want it to loop around edges, 
                                                                                                you give it the tag "Clonable" and it will be in this array. */

        foreach (GameObject clonable_object in list_of_clonable_objects)    // Generate the 9 clones(phantoms) for each GameObject tagged as clonable.
        {
            List<GameObject> phantoms_and_original = new List<GameObject>(); // The group of phantom and their respective original are stored together
                                                                             // in seperate lists so the code can keep track of them.
            
            Color original_color = Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f); // assign to clonable object a random colour
            Color phantom_color = original_color; phantom_color.a = 0.3f; // the phantoms are always more translucent than the original

            var phantoms_and_original_under_a_GameObject = new GameObject(); // The phantoms and their original are placed under an
                                                                             // empty GameObject so the scene hierarchy remains neat and tidy.

            foreach (string offset_setting in phantom_offset_settings)  /* Since the 8 phantom represents the 8 cardinal directions, 
                                                                           they need to be set a fixed distance away from the original
                                                                           (specifically the width and breadth of the gameboard.) 
                                                                           Each cardinal direction associated with a phantom is represented
                                                                           by a short string in a 8 element long array so we dont need to use
                                                                           a long if else chain to move the phantoms.*/
            {
                GameObject original = clonable_object;
                if (offset_setting == "O"){     // "O" represents the original object and is there to exclude it from teleportation.
                    Renderer original_renderer = original.GetComponent<Renderer>();
                    original_renderer.material.color = original_color;
                    original.transform.parent = phantoms_and_original_under_a_GameObject.transform;
                    phantoms_and_original_under_a_GameObject.name = original.name;

                    original.tag = "Original Cloned Object";    // We change the tag of the original object to mark it as already undergone the cloning process so it doesn't repeat forever.
                    phantoms_and_original.Add(original);
                }
                else {
                    GameObject phantom = Instantiate(original, original.transform.position + get_positional_offset_of_phantom(offset_setting), original.transform.rotation);

                    Renderer phantom_renderer = phantom.GetComponent<Renderer>();
                    phantom_renderer.material.color = phantom_color;
                    phantom.transform.parent = phantoms_and_original_under_a_GameObject.transform;

                    phantom.tag = "Phantom";
                    phantoms_and_original.Add(phantom);
                }

                //phantom.transform.parent = original.transform;
            }
            list_of_phantoms_and_their_originals.Add(phantoms_and_original);
        }

        foreach (List<GameObject> phantoms_and_original in list_of_phantoms_and_their_originals){
            float original_x = 0f;
            float original_y = 0f;
            float original_z = 0f;
            GameObject original = phantoms_and_original[4];
            GameObject phantom;

            for (int i = 0; i <= 8; i++) 
            {
                string offset_setting = phantom_offset_settings[i];

                if (offset_setting != "O") {
                    phantom = phantoms_and_original[i];

                    ensure_offset_and_same_height_between_original_and_phantom(original, phantom, phantom_offset_settings[i]);   // Here we ensure each phantom has the same height from the ground as the original.
                    
                }
            }
            if (original_x == 0f){ original_x = original.transform.position.x;}
            if (original_y == 0f){ original_y = original.transform.position.y;}
            if (original_z == 0f){ original_z = original.transform.position.z;}

            /*
            012
            345
            678
            */

            if (original_x > x_offset / 2){     // This is where the actual teleporting takes place. Once the original hits an edge, it is moved to the place of a phantom on the opposite side.
                original.transform.position = phantoms_and_original[5].transform.position;
            } else if (original_x < -x_offset / 2){
                original.transform.position = phantoms_and_original[3].transform.position;
            } else if (original_z > z_offset / 2){
                original.transform.position = phantoms_and_original[7].transform.position;
            } else if (original_z < -z_offset / 2){
                original.transform.position = phantoms_and_original[1].transform.position;
            }
        }
    }

    void update_dimensions(){   // This is to account for how the x, y and z bounds of the playing board temporarily changes as it tilts.

        float x_rotation = this.transform.localRotation.eulerAngles.x;
        float y_rotation = this.transform.localRotation.eulerAngles.y;
        float z_rotation = this.transform.localRotation.eulerAngles.z;

        x_offset = this.GetComponent<MeshRenderer>().bounds.size.x * Mathf.Cos(x_rotation);
        y_offset = this.GetComponent<MeshRenderer>().bounds.size.y * Mathf.Cos(y_rotation);
        z_offset = this.GetComponent<MeshRenderer>().bounds.size.z * Mathf.Cos(z_rotation);
    }

    void ensure_offset_and_same_height_between_original_and_phantom(GameObject original, GameObject phantom, string phantom_offset){    /* What it does is to measure the distance from the original
                                                                                                                                           to the ground with a raycast (light shooting a beam of
                                                                                                                                           light directly downwards), do the same for the phantoms
                                                                                                                                           and adjust the hieght of phantoms to match.*/

        float original_distance_from_ground = 0f;
        float phantom_distance_from_ground = 0f;

        float phantom_y_offset = 0f;

        phantom.transform.position = new Vector3(original.transform.position.x, phantom.transform.position.y, original.transform.position.z) + get_positional_offset_of_phantom(phantom_offset);

        RaycastHit hit;
        if(Physics.Raycast(original.transform.position, Vector3.down, out hit)){
            original_distance_from_ground = hit.distance;
        } else {
            original_distance_from_ground = 0f;
        }
        
        if(Physics.Raycast(phantom.transform.position, Vector3.down, out hit)){
            phantom_distance_from_ground = hit.distance;
        } else {
            phantom_distance_from_ground = 0f;
        }

        /*
        phantom_y_offset = original_distance_from_ground - phantom_distance_from_ground;
        phantom.transform.position = new Vector3(phantom.transform.position.x, phantom.transform.position.y + phantom_y_offset, phantom.transform.position.z);
        */

        if (phantom_distance_from_ground != Mathf.Infinity) {
            phantom_y_offset = original_distance_from_ground - phantom_distance_from_ground;
            phantom.transform.position = new Vector3(phantom.transform.position.x, phantom.transform.position.y + phantom_y_offset, phantom.transform.position.z);
        } else {
            if(Physics.Raycast(new Vector3(phantom.transform.position.x, height_reference, phantom.transform.position.z), Vector3.down, out hit)){
                phantom_distance_from_ground = hit.distance;
            }
            phantom_y_offset = original_distance_from_ground - phantom_distance_from_ground;
            phantom.transform.position = new Vector3(phantom.transform.position.x, height_reference + phantom_y_offset, phantom.transform.position.z);
        }

        phantom.transform.rotation = original.transform.rotation;
    }


    Vector3 get_positional_offset_of_phantom(string offset_setting){    // This is here to avoid long if else statements.
        float phantom_x_offset = 0;
        float phantom_z_offset = 0;

        if (offset_setting.Contains("z")){
            phantom_z_offset = -z_offset;
        } else if (offset_setting.Contains("Z")){
            phantom_z_offset = z_offset;
        }

        if (offset_setting.Contains("x")){
            phantom_x_offset = -x_offset;
        } else if (offset_setting.Contains("X")){
            phantom_x_offset = x_offset;
        }
        
        return new Vector3(phantom_x_offset, 0, phantom_z_offset);
    }
}
